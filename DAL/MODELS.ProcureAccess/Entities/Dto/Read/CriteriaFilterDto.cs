// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Dto;

public class CriteriaFilterDto : BaseDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? FilterTypeId { get; set; }
    public List<LookupDto> Exclusions { get; set; } = new();
}
