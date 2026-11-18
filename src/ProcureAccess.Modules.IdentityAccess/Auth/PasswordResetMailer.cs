// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Threading.Channels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Email;
using IEmailSender = ProcureAccess.Modules.Platform.Contracts.Email.IEmailSender;

namespace ProcureAccess.Modules.IdentityAccess.Auth;

/// <summary>
/// Bounded in-memory queue between <c>POST /api/v1/auth/password-reset</c> and
/// <see cref="PasswordResetMailer"/> (owner decision S5-13a (a), ARCHITECTURE C141): the request
/// only queues the address, so a known address answers as fast as an unknown one. Addresses in it
/// are lost when the process ends; the user asks again, as after a failed send.
/// </summary>
internal sealed class PasswordResetMailQueue
{
    /// <summary>
    /// Addresses waiting at most. The rate limits (5 per hour per client IP 🔧) keep it far below
    /// this; beyond it a request is dropped and logged, the answer stays the same.
    /// </summary>
    internal const int Capacity = 100;

    private readonly Channel<string> _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(Capacity)
    {
        FullMode = BoundedChannelFullMode.Wait, // TryWrite answers false instead of dropping an older entry
        SingleReader = true,
        SingleWriter = false,
    });

    private long _dropped;

    internal ChannelReader<string> Reader => _channel.Reader;

    /// <summary>Queues <paramref name="address"/> without waiting; counts it as dropped when the queue is full.</summary>
    public void Enqueue(string address)
    {
        if (!_channel.Writer.TryWrite(address))
        {
            Interlocked.Increment(ref _dropped);
        }
    }

    /// <summary>Requests dropped since the last call; resets the count.</summary>
    internal long TakeDroppedCount() => Interlocked.Exchange(ref _dropped, 0);
}

/// <summary>
/// Sends the password-reset e-mails of <see cref="PasswordResetMailQueue"/> (SPEC R-AUTH-04; S5-13a):
/// for an address with a non-deleted account — confirmed or not (owner decision S5-13 2 (a)) — it
/// issues a single-use link (🔧 2 h) and sends <see cref="PasswordResetEmail"/> to the stored
/// address; for any other address it does nothing. Logs never name the address (PRV-11).
/// </summary>
internal sealed partial class PasswordResetMailer(
    PasswordResetMailQueue queue,
    IServiceScopeFactory scopes,
    IConfiguration configuration,
    ILogger<PasswordResetMailer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var address in queue.Reader.ReadAllAsync(stoppingToken))
            {
                if (queue.TakeDroppedCount() is var dropped and > 0)
                {
                    LogDropped(dropped);
                }

                try
                {
                    await SendAsync(address, stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    // An unhandled exception would stop the host (BackgroundService default).
                    LogFailed(ex);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The host stops; queued requests are lost, as the class says.
        }
    }

    private async Task SendAsync(string address, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByEmailAsync(address);
        if (user is null || user.IsDeleted || string.IsNullOrEmpty(user.Email))
        {
            return;
        }

        var tokens = scope.ServiceProvider.GetRequiredService<IAccountTokens>();
        var issued = await tokens.IssueAsync(user.Id, AccountTokenPurpose.PasswordReset, cancellationToken);
        var link = ClientLinks.To(configuration, ClientLinks.PasswordResetPagePath + "?token=" + Uri.EscapeDataString(issued.Token));

        try
        {
            await scope.ServiceProvider.GetRequiredService<IEmailSender>()
                       .SendAsync(new PasswordResetEmail(user.Email, link, issued.ValidHours), cancellationToken);
        }
        catch (EmailDeliveryException)
        {
            LogNotSent();
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "A password reset e-mail could not be sent.")]
    private partial void LogNotSent();

    [LoggerMessage(Level = LogLevel.Error, Message = "A password reset request could not be processed.")]
    private partial void LogFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{DroppedCount} password reset requests were dropped because the queue was full.")]
    private partial void LogDropped(long droppedCount);
}
