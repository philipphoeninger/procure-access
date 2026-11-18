// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Email;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Platform;

/// <summary>
/// The 🔧 <c>Email:Smtp:Timeout</c> per SMTP operation (S6-05, TD-58, owner decision O-S6-3) on the
/// real MailKit transport: a fake server on the loopback accepts the connection and then stalls —
/// before its greeting, at the sign-in or at the send. Each attempt ends after the timeout instead
/// of MailKit's default of two minutes, and the sender gives up after its two attempts. The tests
/// use a timeout of one second; the value of <c>appsettings.json</c> is checked in
/// <c>EmailSettingsTests</c>.
/// </summary>
public sealed class SmtpTimeoutTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    /// <summary>Far below MailKit's default of 120 s per operation, far above the 1 s of the tests.</summary>
    private static readonly TimeSpan Bound = TimeSpan.FromSeconds(10);

    [Theory]
    [InlineData(StallPoint.BeforeGreeting)]
    [InlineData(StallPoint.AtSignIn)]
    [InlineData(StallPoint.AtSend)]
    public async Task Transport_ServerStalls_EndsTheAttemptAfterTheTimeout(StallPoint stall)
    {
        await using var server = StalledSmtpServer.Start(stall);
        var settings = Settings(server.Port, withCredentials: stall == StallPoint.AtSignIn);
        var transport = new MailKitSmtpTransport(settings);
        var composer = new EmailComposer(EmailTexts.LoadGerman());
        var mime = composer.Build(Message(), settings.Sender, DateTimeOffset.UtcNow);

        var watch = Stopwatch.StartNew();
        await Should.ThrowAsync<TimeoutException>(() => transport.SendAsync(mime, TestContext.Current.CancellationToken));
        watch.Stop();

        server.ReachedStallPoint.ShouldBeTrue();
        watch.Elapsed.ShouldBeGreaterThanOrEqualTo(Timeout - TimeSpan.FromMilliseconds(100));
        watch.Elapsed.ShouldBeLessThan(Bound);
    }

    [Fact]
    public async Task Send_ServerStalls_EndsWithTheDeliveryFailureAfterBothAttempts()
    {
        await using var server = StalledSmtpServer.Start(StallPoint.BeforeGreeting);
        var settings = Settings(server.Port, withCredentials: false);
        var sender = new SmtpEmailSender(
            new EmailComposer(EmailTexts.LoadGerman()),
            settings,
            new MailKitSmtpTransport(settings),
            TimeProvider.System,
            NullLogger<SmtpEmailSender>.Instance);

        var watch = Stopwatch.StartNew();
        await Should.ThrowAsync<EmailDeliveryException>(() => sender.SendAsync(Message(), TestContext.Current.CancellationToken));
        watch.Stop();

        server.Connections.ShouldBe(SmtpEmailSender.Attempts);
        watch.Elapsed.ShouldBeLessThan(Bound);
    }

    public enum StallPoint
    {
        BeforeGreeting,
        AtSignIn,
        AtSend,
    }

    private static AddressConfirmationEmail Message() =>
        new("ada@example.test", new Uri("https://procureaccess.example/confirm-email?token=t"), 24);

    private static EmailSettings Settings(int port, bool withCredentials)
    {
        var values = new Dictionary<string, string?>
        {
            ["Email:Smtp:Host"] = IPAddress.Loopback.ToString(),
            ["Email:Smtp:Port"] = port.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["Email:Smtp:Security"] = "None",
            ["Email:Smtp:Timeout"] = Timeout.ToString("c", System.Globalization.CultureInfo.InvariantCulture),
            ["Email:From"] = "noreply@procureaccess.example",
        };
        if (withCredentials)
        {
            values["Email:Smtp:Username"] = "user";
            values["Email:Smtp:Password"] = "password";
        }

        return EmailSettings.From(
            new ConfigurationBuilder().AddInMemoryCollection(values).Build(),
            new HostingEnvironment { EnvironmentName = "Development" });
    }

    /// <summary>
    /// An SMTP server that accepts every connection and answers until <see cref="StallPoint"/>, then
    /// reads on and never answers again — as a server that hangs, not one that closes.
    /// </summary>
    private sealed class StalledSmtpServer : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new();
        private readonly List<TcpClient> _clients = [];
        private readonly StallPoint _stall;
        private Task _accepting = Task.CompletedTask;
        private int _connections;
        private volatile bool _reachedStallPoint;

        private StalledSmtpServer(StallPoint stall) => _stall = stall;

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public int Connections => Volatile.Read(ref _connections);

        public bool ReachedStallPoint => _reachedStallPoint;

        public static StalledSmtpServer Start(StallPoint stall)
        {
            var server = new StalledSmtpServer(stall);
            server._listener.Start();
            server._accepting = server.AcceptAsync();
            return server;
        }

        public async ValueTask DisposeAsync()
        {
            await _stop.CancelAsync();
            _listener.Stop();
            try
            {
                await _accepting;
            }
            catch (Exception ex) when (ex is OperationCanceledException or SocketException or ObjectDisposedException)
            {
                // The listener stopped.
            }

            lock (_clients)
            {
                _clients.ForEach(c => c.Dispose());
            }

            _stop.Dispose();
        }

        private async Task AcceptAsync()
        {
            while (!_stop.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_stop.Token);
                Interlocked.Increment(ref _connections);
                lock (_clients)
                {
                    _clients.Add(client);
                }

                _ = ConverseAsync(client.GetStream());
            }
        }

        private async Task ConverseAsync(NetworkStream stream)
        {
            try
            {
                using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
                if (_stall == StallPoint.BeforeGreeting)
                {
                    await StallAsync(reader);
                    return;
                }

                await WriteAsync(stream, "220 fake.test ESMTP\r\n");
                while (await reader.ReadLineAsync(_stop.Token) is { } line)
                {
                    if (line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteAsync(stream, "250-fake.test\r\n250 AUTH PLAIN LOGIN\r\n");
                    }
                    else if (line.StartsWith("AUTH", StringComparison.OrdinalIgnoreCase) && _stall == StallPoint.AtSignIn)
                    {
                        await StallAsync(reader);
                        return;
                    }
                    else if (line.StartsWith("MAIL FROM", StringComparison.OrdinalIgnoreCase) && _stall == StallPoint.AtSend)
                    {
                        await StallAsync(reader);
                        return;
                    }
                    else
                    {
                        await WriteAsync(stream, "502 not implemented\r\n");
                    }
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or IOException or ObjectDisposedException)
            {
                // The client gave up or the test ended.
            }
        }

        /// <summary>Reads whatever the client sends and never answers.</summary>
        private async Task StallAsync(StreamReader reader)
        {
            _reachedStallPoint = true;
            while (await reader.ReadLineAsync(_stop.Token) is not null)
            {
            }
        }

        private Task WriteAsync(NetworkStream stream, string text) =>
            stream.WriteAsync(Encoding.ASCII.GetBytes(text), _stop.Token).AsTask();
    }
}
