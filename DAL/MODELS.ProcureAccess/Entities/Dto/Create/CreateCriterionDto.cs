// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Dto;

public class CreateCriterionDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(6000)]
    public string Description { get; set; } = null!;

    public int CriteriaFilterId { get; set; }
}
