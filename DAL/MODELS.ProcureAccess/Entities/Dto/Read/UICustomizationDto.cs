// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Dto;

public class UICustomizationDto
{
    public string? BackgroundColor { get; set; }
    public string? ForegroundColor { get; set; }
    public string? TextColor { get; set; }
    public bool? DarkModeOn { get; set; }
    public bool? HighContrastOn { get; set; }
    public string? Language { get; set; }
}
