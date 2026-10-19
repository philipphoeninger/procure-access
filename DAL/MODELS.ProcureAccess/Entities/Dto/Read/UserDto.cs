// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Dto;

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public UICustomizationDto? UICustomization { get; set; }
}
