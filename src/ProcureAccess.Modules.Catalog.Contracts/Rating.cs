// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Contracts;

/// <summary>
/// The six rating levels of BIT inklusiv as a set (Kriterienkatalog §5, ARCHITECTURE C25): which
/// levels a procedure text allows for a criterion (<c>CriterionProcedureText.AllowedRatings</c>).
/// „Nicht eingeschätzt“ (<c>Unknown</c>) is not a level of BIT inklusiv and is always allowed, so it
/// has no flag here (SPEC §8.2). In <c>Contracts</c> because Workspace and Directory validate
/// assessments against it (CAT-15; S3-11 §2.2).
/// </summary>
[Flags]
public enum Rating
{
    None = 0,

    /// <summary>Erfüllt.</summary>
    Fulfilled = 1,

    /// <summary>Leichte Einschränkung.</summary>
    SlightRestriction = 2,

    /// <summary>Einschränkung.</summary>
    Restriction = 4,

    /// <summary>Barriere.</summary>
    Barrier = 8,

    /// <summary>Blockade.</summary>
    Blockade = 16,

    /// <summary>Nicht anwendbar.</summary>
    NotApplicable = 32,

    /// <summary>All six levels: what applies when no procedure text does (ARCHITECTURE §5.1, C31).</summary>
    All = Fulfilled | SlightRestriction | Restriction | Barrier | Blockade | NotApplicable,
}
