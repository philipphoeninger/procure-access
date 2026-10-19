namespace MODELS.ProcureAccess.SeedData;

public static class CriteriaFilterExclusionSeed
{
    public static readonly CriteriaFilterExclusion[] Data =
    [
        // HR application - Application Types
        new() { CriteriaFilterId = 17, ExclusionId = 9 },
        new() { CriteriaFilterId = 17, ExclusionId = 12 },
        new() { CriteriaFilterId = 17, ExclusionId = 13 },
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
