// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Tests.Shared.Builders.Catalog;

/// <summary>
/// Builds a <see cref="Criterion"/> of the Catalog module: a requirement criterion by default, a
/// procedure criterion with <see cref="ProcedureFor"/> (key <c>PROC-&lt;testTypeKey&gt;</c>,
/// ARCHITECTURE §5.1).
/// </summary>
internal sealed class CatalogCriterionBuilder : Builder<Criterion>
{
    private string? _key;
    private string? _category;
    private string? _supersededByKey;
    private TestType? _testType;

    public CatalogCriterionBuilder WithKey(string key)
    {
        _key = key;
        return this;
    }

    public CatalogCriterionBuilder WithCategory(string category)
    {
        _category = category;
        return this;
    }

    /// <summary>Points the criterion at the key that replaces it in a later release.</summary>
    public CatalogCriterionBuilder SupersededBy(string key)
    {
        _supersededByKey = key;
        return this;
    }

    /// <summary>Makes it the procedure criterion of <paramref name="testType"/>.</summary>
    public CatalogCriterionBuilder ProcedureFor(TestType testType)
    {
        _testType = testType;
        return this;
    }

    public override Criterion Build() =>
        _testType is null
            ? Criterion.Create(_key ?? "T-CRITERION", CriterionKind.Requirement, _category, _supersededByKey)
            : Criterion.Create(
                _key ?? $"PROC-{_testType.Key}", CriterionKind.TestProcedure, _category, _supersededByKey, _testType);
}
