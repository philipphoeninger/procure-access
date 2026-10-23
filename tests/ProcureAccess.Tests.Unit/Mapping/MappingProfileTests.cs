// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using AutoMapper;
using MODELS.ProcureAccess.Entities.Mapping;
using Xunit;

namespace ProcureAccess.Tests.Unit.Mapping;

public class MappingProfileTests
{
    [Fact(DisplayName = "INV:ARC-06 " + nameof(MappingConfiguration_AllProfilesOfTheApi_IsValid))]
    [Trait("Invariant", "ARC-06")]
    public void MappingConfiguration_AllProfilesOfTheApi_IsValid()
    {
        // Same profile scan as AddAutoMapper in API/API.ProcureAccess/Program.cs.
        var configuration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

        configuration.AssertConfigurationIsValid();
    }
}
