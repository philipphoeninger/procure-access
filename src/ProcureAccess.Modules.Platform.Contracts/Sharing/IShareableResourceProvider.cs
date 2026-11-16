// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Sharing;

/// <summary>
/// The owning module's side of a share link (ARCHITECTURE §5.3, C59): Platform resolves the token
/// of <c>GET /api/v1/shared/{token}</c> to a resource and asks the provider of its
/// <see cref="ResourceType"/> for the redacted read. Declared here, so Platform depends on no
/// module; implemented by the module that owns the resource (Configurator for the saved lists).
/// </summary>
public interface IShareableResourceProvider
{
    /// <summary>The kind of resource this provider reads.</summary>
    ShareableResourceType ResourceType { get; }

    /// <summary>
    /// The read-only view of the resource for someone without an account, or <c>null</c> when it
    /// no longer exists (a deleted list: its links answer 404, SPEC R-SAV-06). The request is
    /// anonymous, so the provider reads past its organisation filter at an allow-listed call site
    /// (TEN-02). The answer carries no organisation, user or project data and no id of the
    /// source (TEN-08); <paramref name="expiresAt"/> is the link's end, which the view names.
    /// </summary>
    Task<object?> ReadSharedAsync(Guid resourceId, DateTimeOffset expiresAt, CancellationToken cancellationToken);
}
