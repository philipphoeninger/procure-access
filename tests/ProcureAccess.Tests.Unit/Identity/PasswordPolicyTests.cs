// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Auth;
using API.ProcureAccess.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MODELS.ProcureAccess.Entities;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Identity;

/// <summary>
/// The password rule of SPEC R-AUTH-01 (S5-12a, ARCHITECTURE §9.1): at least 🔧 12 characters, at
/// most 128, no composition rules.
/// </summary>
public sealed class PasswordPolicyTests
{
    [Fact]
    public void IdentityOptions_ConfiguredMinimum_NoCompositionRules()
    {
        var services = new ServiceCollection().ConfigureIdentityOptions(Configuration("12"));
        var password = services.BuildServiceProvider().GetRequiredService<IOptions<IdentityOptions>>().Value.Password;

        password.RequiredLength.ShouldBe(12);
        password.RequireDigit.ShouldBeFalse();
        password.RequireLowercase.ShouldBeFalse();
        password.RequireUppercase.ShouldBeFalse();
        password.RequireNonAlphanumeric.ShouldBeFalse();
        password.RequiredUniqueChars.ShouldBe(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("129")]
    [InlineData("twelve")]
    public void MinLengthFrom_MissingOrImpossibleValue_StopsTheStart(string? value) =>
        Should.Throw<InvalidOperationException>(() => PasswordPolicy.MinLengthFrom(Configuration(value)))
              .Message.ShouldContain(PasswordPolicy.MinLengthSetting);

    [Fact]
    public void MinLength_DefaultOfAppsettings_Is12()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build();

        PasswordPolicy.MinLengthFrom(configuration).ShouldBe(12);
    }

    [Theory]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public async Task MaximumLengthValidator_Length_RefusesOnlyAbove128(int length, bool valid)
    {
        var result = await new MaximumLengthPasswordValidator().ValidateAsync(null!, new User(), new string('x', length));

        result.Succeeded.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.Single().Code.ShouldBe(MaximumLengthPasswordValidator.ErrorCode);
        }
    }

    private static IConfiguration Configuration(string? minLength) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [PasswordPolicy.MinLengthSetting] = minLength })
            .Build();
}
