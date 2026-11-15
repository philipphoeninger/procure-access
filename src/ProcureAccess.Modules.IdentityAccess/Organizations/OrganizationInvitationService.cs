// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Buffers.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Organizations;

/// <summary>
/// <see cref="IOrganizationInvitations"/> on <see cref="IdentityAccessDbContext"/> (S5-17a). The
/// link's token is made like the account tokens (<see cref="AccountTokenService"/>: 32 random bytes as
/// Base64url, only their SHA-256 stored). Inviting, revoking and accepting run in a transaction under
/// the organisation's row lock (<see cref="OrganizationRowLock"/>), so two invitations of one address
/// never both stay open and a link is accepted at most once, also by two requests at the same moment.
/// </summary>
internal sealed class OrganizationInvitationService(
    IdentityAccessDbContext db,
    InvitationSettings settings,
    TimeProvider clock) : IOrganizationInvitations
{
    public TimeSpan Lifetime => settings.Lifetime;

    public async Task<IssuedInvitation?> InviteAsync(
        Guid organizationId, string email, OrganizationRole role, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown organisation role.");
        }

        var bytes = RandomNumberGenerator.GetBytes(AccountTokenService.TokenBytes);

        // The context retries on transient failures, so a transaction of our own runs inside its
        // execution strategy, and a retry starts again from the database.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async ct =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct);

                // Visible through the filter first: only a member's request takes the lock.
                var name = await db.Organizations.Where(o => o.Id == organizationId).Select(o => o.Name).SingleOrDefaultAsync(ct);
                if (name is null)
                {
                    return null;
                }

                await OrganizationRowLock.LockAsync(db, organizationId, ct);

                var invitation = OrganizationInvitation.Create(
                    organizationId, email.Trim(), role, SHA256.HashData(bytes), clock.GetUtcNow(), settings.Lifetime);

                // A new invitation replaces the open one of the same address (owner decision S5-17 D3 (a)).
                await db.OrganizationInvitations
                        .Where(i => i.OrganizationId == organizationId
                                    && i.NormalizedEmail == invitation.NormalizedEmail
                                    && i.AcceptedAt == null)
                        .ExecuteDeleteAsync(ct);
                db.OrganizationInvitations.Add(invitation);
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return new IssuedInvitation(invitation.Id, name, Base64Url.EncodeToString(bytes), invitation.ExpiresAt, settings.Lifetime);
            },
            cancellationToken);
    }

    public async Task<IReadOnlyList<PendingInvitation>?> GetPendingAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        if (!await db.Organizations.AnyAsync(o => o.Id == organizationId, cancellationToken))
        {
            return null;
        }

        var now = clock.GetUtcNow();
        var pending = await db.OrganizationInvitations.AsNoTracking()
                              .Where(i => i.OrganizationId == organizationId && i.AcceptedAt == null && i.ExpiresAt > now)
                              .Select(i => new PendingInvitation(i.Id, i.Email, i.Role, i.ExpiresAt))
                              .ToListAsync(cancellationToken);
        return [.. pending.OrderBy(i => i.Email, StringComparer.OrdinalIgnoreCase).ThenBy(i => i.Id)];
    }

    public async Task<bool> RevokeAsync(Guid organizationId, Guid invitationId, CancellationToken cancellationToken)
    {
        // Under the lock too: a revocation between an acceptance's reload and its save would make
        // that save find no row (a concurrency exception) instead of one clear outcome.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async ct =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                if (!await db.Organizations.AnyAsync(o => o.Id == organizationId, ct))
                {
                    return false;
                }

                await OrganizationRowLock.LockAsync(db, organizationId, ct);
                var deleted = await db.OrganizationInvitations
                                      .Where(i => i.OrganizationId == organizationId && i.Id == invitationId && i.AcceptedAt == null)
                                      .ExecuteDeleteAsync(ct);
                await transaction.CommitAsync(ct);
                return deleted == 1;
            },
            cancellationToken);
    }

    public async Task<InvitationDetails?> FindAsync(string? token, string? callerEmail, CancellationToken cancellationToken)
    {
        if (!AccountTokenService.TryHash(token, out var hash))
        {
            return null;
        }

        var found = await FindByTokenAsync(hash, cancellationToken);
        if (found is null)
        {
            return null;
        }

        var invitation = found.Invitation;
        var member = await IsMemberAsync(invitation.OrganizationId, cancellationToken);
        var usable = invitation.IsOpenAt(clock.GetUtcNow()) || (invitation.AcceptedAt is not null && member);
        if (!usable)
        {
            return null;
        }

        return new InvitationDetails(
            invitation.OrganizationId,
            found.OrganizationName,
            invitation.Role,
            invitation.Email,
            callerEmail is null ? null : invitation.IsFor(callerEmail),
            member);
    }

    public async Task<InvitationAcceptance> AcceptAsync(
        string? token, string userId, string userEmail, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        if (!AccountTokenService.TryHash(token, out var hash))
        {
            return InvitationAcceptance.Invalid;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async ct =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct);

                var found = await FindByTokenAsync(hash, ct);
                if (found is null)
                {
                    return InvitationAcceptance.Invalid;
                }

                var invitation = found.Invitation;
                var organizationId = invitation.OrganizationId;
                await OrganizationRowLock.LockAsync(db, organizationId, ct);

                // The row as it is now: an acceptance, revocation or replacement committed while this
                // request waited for the lock counts. A deleted row is detached by the reload.
                var entry = db.Entry(invitation);
                await entry.ReloadAsync(ct);
                if (entry.State == EntityState.Detached)
                {
                    return InvitationAcceptance.Invalid;
                }

                // Through the filter: the memberships the request's user had when the request started.
                var member = await IsMemberAsync(organizationId, ct);
                if (invitation.AcceptedAt is not null)
                {
                    return member
                        ? new InvitationAcceptance(InvitationAcceptanceOutcome.AlreadyMember, organizationId)
                        : InvitationAcceptance.Invalid;
                }

                var now = clock.GetUtcNow();
                if (!invitation.IsOpenAt(now))
                {
                    return InvitationAcceptance.Invalid;
                }

                if (!invitation.IsFor(userEmail))
                {
                    return new InvitationAcceptance(InvitationAcceptanceOutcome.AddressMismatch, InvitedEmail: invitation.Email);
                }

                invitation.Accept(now);
                if (!member)
                {
                    db.OrganizationMemberships.Add(
                        OrganizationMembership.Create(organizationId, userId, invitation.Role, invitation.CreatedAt, now));
                }

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return new InvitationAcceptance(
                    member ? InvitationAcceptanceOutcome.AlreadyMember : InvitationAcceptanceOutcome.Accepted, organizationId);
            },
            cancellationToken);
    }

    /// <summary>True when the request's user is a member of <paramref name="organizationId"/> (the filter shows it).</summary>
    private Task<bool> IsMemberAsync(Guid organizationId, CancellationToken cancellationToken) =>
        db.Organizations.AnyAsync(o => o.Id == organizationId, cancellationToken);

    /// <summary>
    /// The invitation of a link with its organisation's name, past the organisation filter: the
    /// invitee is no member yet, and the token, which only the invited mailbox received, is the
    /// credential (TEN-02 allow-list). Tracked, so accepting can change it.
    /// </summary>
    private async Task<TokenMatch?> FindByTokenAsync(byte[] hash, CancellationToken cancellationToken)
    {
        var match = await db.OrganizationInvitations.IgnoreQueryFilters()
                            .Where(i => i.TokenHash == hash)
                            .Join(db.Organizations, i => i.OrganizationId, o => o.Id, (i, o) => new { Invitation = i, o.Name })
                            .SingleOrDefaultAsync(cancellationToken);
        return match is null ? null : new TokenMatch(match.Invitation, match.Name);
    }

    private sealed record TokenMatch(OrganizationInvitation Invitation, string OrganizationName);
}
