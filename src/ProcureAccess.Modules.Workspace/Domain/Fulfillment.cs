// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// How a candidate product fulfils a requirement criterion (ARCHITECTURE §5.4; owner decision of
/// 2026-10-06): the six levels of BIT inklusiv and „Nicht eingeschätzt“
/// (<c>assessment.value.*</c>). Stored by name.
/// </summary>
/// <remarks>
/// <see cref="Unknown"/> is the value of a criterion without a row and never means „not
/// fulfilled“. An <i>assessment exists</i> only with another value (C53, WSP-02).
/// </remarks>
internal enum Fulfillment
{
    /// <summary>„Nicht eingeschätzt“; always allowed (C31).</summary>
    Unknown,

    /// <summary>„Erfüllt“.</summary>
    Fulfilled,

    /// <summary>„Leichte Einschränkung“.</summary>
    SlightRestriction,

    /// <summary>„Einschränkung“.</summary>
    Restriction,

    /// <summary>„Barriere“.</summary>
    Barrier,

    /// <summary>„Blockade“.</summary>
    Blockade,

    /// <summary>„Nicht anwendbar“.</summary>
    NotApplicable,
}
