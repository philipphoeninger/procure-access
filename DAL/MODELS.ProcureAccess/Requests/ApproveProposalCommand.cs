// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Requests;

public class ApproveProposalCommand
{
    public int ProposalId { get; set; }
    public bool IsApproved { get; set; }
    public string? Note { get; set; }
}
