// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Contracts;

/// <summary>
/// The test procedures of the catalog („Prüfverfahren“, layer 4; ARCHITECTURE §5.1, C27) by key,
/// for modules that name one outside a selection — a test evidence of a candidate product names the
/// procedure it was tested by (§5.4, C46; S6-15). The procedures are filter values, which belong to
/// no release (C104), so a key stays valid across releases.
/// </summary>
public interface ICatalogTestTypes
{
    /// <summary>The keys of every test procedure, e.g. <c>softwareTest</c>, in catalog order.</summary>
    Task<IReadOnlyList<string>> GetKeysAsync(CancellationToken cancellationToken);
}
