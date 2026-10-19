// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Dto;

public class ProposalDto : BaseDto
{
    public string ProposerId { get; set; }
    public string? ApproverId { get; set; }
    public ProductDto? Product { get; set; }
    public CriterionDto? Criterion { get; set; }
    public ProposalStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
