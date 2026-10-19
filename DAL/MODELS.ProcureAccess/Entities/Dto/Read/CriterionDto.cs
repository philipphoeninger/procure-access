// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Dto;

public class CriterionDto : BaseDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [StringLength(6000)]
    public string Description { get; set; }

    public int CriteriaFilterId { get; set; }
}
