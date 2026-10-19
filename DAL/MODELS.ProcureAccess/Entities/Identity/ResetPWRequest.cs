// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Identity;

public class ResetPWRequest
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
}
