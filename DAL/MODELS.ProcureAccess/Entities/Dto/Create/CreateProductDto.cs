// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Dto;

public class CreateProductDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Url]
    [StringLength(500)]
    public string? Link { get; set; }
    
    [StringLength(6000)]
    public string? Description { get; set; }

    public List<int> CriteriaFilterIds { get; set; } = [];
}
