// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Controllers;
using API.ProcureAccess.Controllers.Base;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Architecture.Api;

/// <summary>
/// ARC-05: <see cref="BaseCrudController{TEntity, TDto}"/> is used only by the catalog-admin
/// controllers (ARCHITECTURE D11). Workspace and directory endpoints are hand-written with their
/// invariants. The list is an allow-list: a new derived controller fails here.
/// </summary>
public class GenericCrudControllerTests
{
    /// <summary>The legacy catalog, read-only since S3-07; they go when the legacy filter goes (M2).</summary>
    private static readonly Type[] CatalogAdminControllers =
        [typeof(CriteriaController), typeof(CriteriaFiltersController), typeof(FilterTypesController)];

    /// <summary>
    /// Deferred part of ARC-05 (TESTING V317, owner decision S6-23 D3 (a)): the legacy product read
    /// belongs to the directory, not to catalog administration, and stays on the generic base until
    /// step 6 retires it. Remove the entry with the controller; never add one.
    /// </summary>
    private static readonly Type[] LegacyExceptions = [typeof(ProductsController)];

    [Fact(DisplayName = "INV:ARC-05 " + nameof(BaseCrudController_DerivedControllers_OnlyCatalogAdmin))]
    [Trait("Invariant", "ARC-05")]
    public void BaseCrudController_DerivedControllers_OnlyCatalogAdmin()
    {
        var derived = typeof(BaseCrudController<,>).Assembly.GetTypes()
            .Where(type => type is { IsClass: true } && DerivesFromGenericBase(type))
            .ToList();

        derived.ShouldBe([.. CatalogAdminControllers, .. LegacyExceptions], ignoreOrder: true);
    }

    [Fact]
    public void DerivesFromGenericBase_ControllerOfAnotherBase_IsNotCounted()
    {
        DerivesFromGenericBase(typeof(CriteriaController)).ShouldBeTrue();
        DerivesFromGenericBase(typeof(Microsoft.AspNetCore.Mvc.ControllerBase)).ShouldBeFalse();
        DerivesFromGenericBase(typeof(GenericCrudControllerTests)).ShouldBeFalse();
    }

    private static bool DerivesFromGenericBase(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(BaseCrudController<,>))
            {
                return true;
            }
        }
        return false;
    }
}
