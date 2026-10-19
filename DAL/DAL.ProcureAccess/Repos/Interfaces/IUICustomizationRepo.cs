// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Repos.Interfaces;

public interface IUICustomizationRepo
{
    Task<UICustomizationDto> GetAsync(string userId);
    Task UpdateAsync(string userId, UICustomizationDto dto);
}
