// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities;

namespace ProcureAccess.Tests.Shared.Builders;

/// <summary>
/// Builds a <see cref="Criterion"/> of the existing data model. Replaced by builders for the
/// catalog-release model in migration step 2.
/// </summary>
/// <remarks>
/// By default the criterion gets its own new <see cref="CriteriaFilter"/> (with a new
/// <see cref="FilterType"/>), so storing it in an empty database inserts the whole chain.
/// <see cref="InCriteriaFilter"/> points it at an existing filter instead.
/// </remarks>
public sealed class CriterionBuilder : Builder<Criterion>
{
    private string _name = "Test criterion";
    private string _description = "Description of the test criterion.";
    private int? _criteriaFilterId;
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

    public override Criterion Build()
    {
        var criterion = new Criterion(_name, _description)
        {
            IsDeleted = _isDeleted,
            CreatedAt = TestData.DefaultCreatedAt,
        };

        if (_criteriaFilterId is int criteriaFilterId)
        {
            criterion.CriteriaFilterId = criteriaFilterId;
        }
        else
        {
            criterion.CriteriaFilter = new CriteriaFilter(
                "Test criteria filter", 0, "Description of the test criteria filter.")
            {
                FilterType = TestData.AFilterType().Build(),
                CreatedAt = TestData.DefaultCreatedAt,
            };
        }

        return criterion;
    }
}
