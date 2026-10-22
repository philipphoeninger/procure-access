// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities;

namespace ProcureAccess.Tests.Shared.Builders;

/// <summary>
/// Builds a <see cref="Criterion"/> of the existing data model. Replaced by builders for the
/// catalog-release model in migration step 2.
/// </summary>
public sealed class CriterionBuilder : Builder<Criterion>
{
    private string _name = "Test criterion";
    private string _description = "Description of the test criterion.";
    private int _criteriaFilterId = 1;
    private bool _isDeleted;

    public CriterionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CriterionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CriterionBuilder InCriteriaFilter(int criteriaFilterId)
    {
        _criteriaFilterId = criteriaFilterId;
        return this;
    }

    public CriterionBuilder Deleted()
    {
        _isDeleted = true;
        return this;
    }

    public override Criterion Build() => new(_name, _description)
    {
        CriteriaFilterId = _criteriaFilterId,
        IsDeleted = _isDeleted,
        CreatedAt = TestData.DefaultCreatedAt,
    };
}
