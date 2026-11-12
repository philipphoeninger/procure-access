// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Builders;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// The generic CRUD controllers are read-only (S3-07, owner decision O-S3-4 (c), #90): their
/// <c>POST</c>, <c>PUT</c> and <c>DELETE</c> were reachable without sign-in. The catalog changes only
/// through the CLI import (SPEC SD20); proposals only through their own endpoints.
/// </summary>
public sealed class GenericCrudWriteTests(ApiFixture api) : IntegrationTest(api)
{
    private static readonly string[] GenericCrudControllers =
        ["Criteria", "CriteriaFilters", "FilterTypes", "Products", "Proposals"];

    private static readonly HashSet<string> WriteMethods = ["POST", "PUT", "PATCH", "DELETE"];

    [Fact]
    public void GenericCrudControllers_EndpointsListed_HaveNoGenericWrite()
    {
        var writes = Api.Factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => (
                Action: endpoint.Metadata.GetMetadata<ControllerActionDescriptor>(),
                Methods: endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods ?? []))
            .Where(e => e.Action is not null && GenericCrudControllers.Contains(e.Action.ControllerName))
            .Where(e => e.Action!.MethodInfo.DeclaringType!.IsGenericType)
            .Where(e => e.Methods.Any(WriteMethods.Contains))
            .Select(e => $"{string.Join(",", e.Methods)} {e.Action!.ControllerName}.{e.Action.ActionName}")
            .ToList();

        writes.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("POST", "/api/FilterTypes")]
    [InlineData("PUT", "/api/FilterTypes/{0}")]
    [InlineData("DELETE", "/api/FilterTypes/{0}")]
    public async Task GenericWrite_Anonymous_IsRejectedAndRecordStays(string method, string pathFormat)
    {
        await AssertWriteRejectedAsync(method, pathFormat, user: null);
    }

    [Theory]
    [InlineData("POST", "/api/FilterTypes")]
    [InlineData("PUT", "/api/FilterTypes/{0}")]
    [InlineData("DELETE", "/api/FilterTypes/{0}")]
    public async Task GenericWrite_SignedInModerator_IsRejectedAndRecordStays(string method, string pathFormat)
    {
        await AssertWriteRejectedAsync(method, pathFormat, TestUsers.Moderator);
    }

    private async Task AssertWriteRejectedAsync(string method, string pathFormat, TestUser? user)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();
        var filterType = new FilterType("Stays", "Must not change") { CreatedAt = TestData.DefaultCreatedAt };
        await StoreAsync(filterType);
        using var client = CreateClient(user);

        using var request = new HttpRequestMessage(new HttpMethod(method), string.Format(pathFormat, filterType.Id))
        {
            Content = new StringContent(
                $$"""{"id":{{filterType.Id}},"name":"Changed","description":"Changed"}""", Encoding.UTF8, "application/json"),
        };
        using var response = await client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        using var read = await client.GetAsync("/api/FilterTypes", cancellationToken);
        var body = await read.Content.ReadAsStringAsync(cancellationToken);
        body.ShouldContain("\"name\":\"Stays\"");
        body.ShouldNotContain("Changed");
    }
}
