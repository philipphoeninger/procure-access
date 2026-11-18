// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Mapping;

/// <summary>
/// Hand-written map of the legacy proposal entity (ADR-015, replaces AutoMapper, #31). ARC-06
/// checks that no target member stays unset. The user's map went with the users into the
/// IdentityAccess module (S6-10).
/// </summary>
public static class AccountMappings
{
    public static ProposalDto ToDto(this Proposal proposal) => new()
    {
        Id = proposal.Id,
        ProposerId = proposal.ProposerId,
        ApproverId = proposal.ApproverId,
        Product = proposal.Product?.ToDto(),
        Criterion = proposal.Criterion?.ToDto(),
        Status = proposal.Status,
        Note = proposal.Note,
        CreatedAt = proposal.CreatedAt,
        FinishedAt = proposal.FinishedAt,
    };
}
