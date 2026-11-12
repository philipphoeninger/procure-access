// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ProcureAccess.Modules.Catalog.Http;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// The language part of a catalog answer that the endpoint tests cannot reach while every release is
/// German (API-06, S4-23): the language is part of the <c>ETag</c>, so the same body in another
/// language never revalidates to 304.
/// </summary>
public class CatalogJsonAnswerTests
{
    private static readonly object Body = new { release = "T.1" };

    [Fact]
    public void Create_SameBodyInAnotherLanguage_HasAnotherETag()
    {
        var german = Answer("de", ifNoneMatch: null);
        var english = Answer("en", ifNoneMatch: null);

        english.Context.Response.Headers.ETag.ToString().ShouldNotBe(german.Context.Response.Headers.ETag.ToString());
        english.Context.Response.Headers.ContentLanguage.ToString().ShouldBe("en");
        english.Context.Response.Headers.Vary.ToString().ShouldBe("Accept-Language");
    }

    [Fact]
    public void Create_ETagOfAnotherLanguage_IsNotRevalidated()
    {
        var germanETag = Answer("de", ifNoneMatch: null).Context.Response.Headers.ETag.ToString();

        Answer("de", germanETag).Result.ShouldBeOfType<StatusCodeHttpResult>().StatusCode.ShouldBe(StatusCodes.Status304NotModified);
        Answer("en", germanETag).Result.ShouldBeOfType<FileContentHttpResult>();
    }

    private static (IResult Result, HttpContext Context) Answer(string language, string? ifNoneMatch)
    {
        var context = new DefaultHttpContext();
        if (ifNoneMatch is not null)
        {
            context.Request.Headers.IfNoneMatch = ifNoneMatch;
        }

        var result = CatalogJsonAnswer.Create(Body, namedRelease: true, language, JsonSerializerOptions.Web, context);
        return (result, context);
    }
}
