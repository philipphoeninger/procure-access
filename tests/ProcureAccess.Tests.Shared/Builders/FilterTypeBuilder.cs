// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities;

namespace ProcureAccess.Tests.Shared.Builders;

/// <summary>
/// Builds a <see cref="FilterType"/> of the existing data model. Replaced by builders for the
/// catalog-release model in migration step 2.
/// </summary>
public sealed class FilterTypeBuilder : Builder<FilterType>
{
    private string _name = "Test filter type";
    private string _description = "Description of the test filter type.";
    private bool _isDeleted;

    public FilterTypeBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public FilterTypeBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public FilterTypeBuilder Deleted()
    {
        _isDeleted = true;
        return this;
    }

    public override FilterType Build() => new(_name, _description)
    {
        IsDeleted = _isDeleted,
        CreatedAt = TestData.DefaultCreatedAt,
    };
}
