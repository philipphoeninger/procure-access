// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.SeedData;

public static class CriteriaFilterExclusionSeed
{
    public static readonly CriteriaFilterExclusion[] Data =
    [
        // HR application - Application Types. It does not exclude the two mobile app types 12 and 13
        // (ARCHITECTURE §16.3.2; removed by the migration RemoveHrMobileExclusions, S3-18).
        new() { CriteriaFilterId = 17, ExclusionId = 9 },
        new() { CriteriaFilterId = 17, ExclusionId = 14 },
        new() { CriteriaFilterId = 17, ExclusionId = 15 },
        // HR application - Test Types
        new() { CriteriaFilterId = 17, ExclusionId = 25 },
        // Website / Webapp - Test Types
        new() { CriteriaFilterId = 8, ExclusionId = 24 },
        new() { CriteriaFilterId = 8, ExclusionId = 25 },
        new() { CriteriaFilterId = 8, ExclusionId = 27 }
    ];
}
