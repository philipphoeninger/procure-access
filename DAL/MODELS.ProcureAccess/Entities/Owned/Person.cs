// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Owned;

[Owned]
public class Person
{
    [Required, StringLength(50)]
    public string? FirstName { get; set; }

    [Required, StringLength(50)]
    public string? LastName { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? FullName { get; set; }
}
