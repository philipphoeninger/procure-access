// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Thrown when code tries to change a published catalog release or a revision that belongs to one
/// (invariant CAT-05). Published content never changes; a correction is a new revision in a new
/// release (ARCHITECTURE §5.1, ADR-002).
/// </summary>
internal sealed class PublishedCatalogChangeException(string message) : InvalidOperationException(message);
