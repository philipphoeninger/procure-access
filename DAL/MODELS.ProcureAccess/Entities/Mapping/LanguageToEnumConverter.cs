// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Mapping;

public class LanguageToEnumConverter : IValueConverter<string, Language>
{
    public Language Convert(string sourceMember, ResolutionContext context)
    {
        return sourceMember?.ToLower() switch
        {
            Languages.En => Language.English,
            Languages.De => Language.German,
            Languages.Es => Language.Spanish,
            Languages.Fr => Language.French,
            _ => Language.German
        };
    }
}
