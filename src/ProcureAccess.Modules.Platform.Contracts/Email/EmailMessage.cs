// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Email;

/// <summary>
/// One e-mail of a message type of ARCHITECTURE C71: the recipient and the values its text needs,
/// nothing more (PRV-11). Classes rather than records on purpose: a record's <c>ToString</c> prints
/// every value, so logging a message would log the recipient and the link with its token.
/// </summary>
/// <remarks>
/// The two product-request types (<c>email.productRequest.*</c>) come with the step that sends them
/// (owner decision S5-11 2a).
/// </remarks>
public abstract class EmailMessage
{
    private protected EmailMessage(string recipient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        Recipient = recipient;
    }

    /// <summary>The address the e-mail goes to. Never logged above <c>Debug</c>.</summary>
    public string Recipient { get; }

    /// <summary>The name of the message type, safe to log.</summary>
    public string MessageType => GetType().Name;

    /// <inheritdoc/>
    public override string ToString() => MessageType;
}

/// <summary>Confirmation of the e-mail address after registration (<c>email.confirm.*</c>, R-AUTH-02).</summary>
public sealed class AddressConfirmationEmail : EmailMessage
{
    public AddressConfirmationEmail(string recipient, Uri link, int validHours)
        : base(recipient)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(validHours);
        Link = link;
        ValidHours = validHours;
    }

    /// <summary>The confirmation link, written out in full.</summary>
    public Uri Link { get; }

    /// <summary>How many hours the link is valid (<c>{{hours}}</c>, 🔧 since S5-12a, UX copy U113).</summary>
    public int ValidHours { get; }
}

/// <summary>Password reset (<c>email.reset.*</c>, R-AUTH-04).</summary>
public sealed class PasswordResetEmail : EmailMessage
{
    public PasswordResetEmail(string recipient, Uri link, int validHours)
        : base(recipient)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(validHours);
        Link = link;
        ValidHours = validHours;
    }

    /// <summary>The reset link, written out in full.</summary>
    public Uri Link { get; }

    /// <summary>How many hours the link is valid (<c>{{hours}}</c>).</summary>
    public int ValidHours { get; }
}

/// <summary>Notice to an address that already has an account (<c>email.alreadyRegistered.*</c>, R-AUTH-01).</summary>
public sealed class AlreadyRegisteredEmail(string recipient, Uri link) : EmailMessage(recipient)
{
    /// <summary>The link to sign-in or the password reset, written out in full.</summary>
    public Uri Link { get; } = link ?? throw new ArgumentNullException(nameof(link));
}

/// <summary>Invitation into an organisation (<c>email.invitation.*</c>, R-ORG-02).</summary>
public sealed class OrganisationInvitationEmail : EmailMessage
{
    public OrganisationInvitationEmail(string recipient, string organisationName, InvitationRole role, Uri link, int validDays)
        : base(recipient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organisationName);
        ArgumentNullException.ThrowIfNull(link);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(validDays);
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown invitation role.");
        }

        OrganisationName = organisationName;
        Role = role;
        Link = link;
        ValidDays = validDays;
    }

    /// <summary>The name of the organisation (<c>{{org}}</c>), also in the subject.</summary>
    public string OrganisationName { get; }

    /// <summary>The role the invitation grants (<c>{{role}}</c>, the label of <c>org.members.role.*</c>).</summary>
    public InvitationRole Role { get; }

    /// <summary>The link that accepts the invitation, written out in full.</summary>
    public Uri Link { get; }

    /// <summary>How many days the invitation is valid (<c>{{days}}</c>).</summary>
    public int ValidDays { get; }
}

/// <summary>
/// The role named in an invitation e-mail; its label is <c>org.members.role.&lt;role&gt;</c>. The
/// organisation module maps its own role to it (S5-17).
/// </summary>
public enum InvitationRole
{
    /// <summary>„Verwalten“</summary>
    Owner,

    /// <summary>„Bearbeiten“</summary>
    Editor,

    /// <summary>„Lesen“</summary>
    Reader,
}
