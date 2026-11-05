// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Mapping;

/// <summary>
/// Hand-written maps of the legacy user, UI settings and proposal entities (ADR-015, replaces
/// AutoMapper, #31). ARC-06 checks that no target member stays unset.
/// </summary>
public static class AccountMappings
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        UICustomization = user.UICustomization.ToDto(),
    };

    public static UICustomizationDto ToDto(this UICustomization settings) => new()
    {
        BackgroundColor = settings.BackgroundColor,
        ForegroundColor = settings.ForegroundColor,
        TextColor = settings.TextColor,
        DarkModeOn = settings.DarkModeOn,
        HighContrastOn = settings.HighContrastOn,
        Language = settings.Language.ToLanguageCode(),
    };

    public static ProposalDto ToDto(this Proposal proposal) => new()
    {
        Id = proposal.Id,
        ProposerId = proposal.ProposerId,
        ApproverId = proposal.ApproverId,
        Product = proposal.Product?.ToDto(),
        Criterion = proposal.Criterion?.ToDto(),
        Status = proposal.Status,
        Note = proposal.Note,
        CreatedAt = proposal.CreatedAt,
        FinishedAt = proposal.FinishedAt,
    };

    /// <summary>Language code of the client (<see cref="Languages"/>); German for an unknown value.</summary>
    public static string ToLanguageCode(this Language language) => language switch
    {
        Language.English => Languages.En,
        Language.German => Languages.De,
        Language.Spanish => Languages.Es,
        Language.French => Languages.Fr,
        _ => Languages.De,
    };

    /// <summary>Language of a client code, case-insensitive; German for an unknown code or <c>null</c>.</summary>
    public static Language ToLanguage(this string? code) => code?.ToLowerInvariant() switch
    {
        Languages.En => Language.English,
        Languages.De => Language.German,
        Languages.Es => Language.Spanish,
        Languages.Fr => Language.French,
        _ => Language.German,
    };
}
