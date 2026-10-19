namespace DAL.ProcureAccess.Initialization;

public static class SampleDataInitializer
{
    #region public
    public static async Task InitializeData(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        await SeedData(dbContext, userManager);
    }

    public static async Task ClearDatabase(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        await ClearData(dbContext, userManager);
    }

    public static async Task ClearAndReseedDatabase(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        await ClearData(dbContext, userManager);
        await SeedData(dbContext, userManager);
    }
    #endregion

    internal static async Task ClearData(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        foreach (User user in userManager.Users)
        {
            await userManager.DeleteAsync(user);
        }
    }

    internal static async Task SeedData(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        try
        {
            var succeeded = await ProcessUserInsert(dbContext, userManager, SampleData.Users);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }

        static async Task<bool> ProcessUserInsert(
            ApplicationDBContext dbContext,
            UserManager<User> userManager,
            List<User> users)
        {
            var allSucceeded = true;
            foreach (User user in users)
            {
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    allSucceeded = false;
                    break;
                }
                await userManager.AddToRoleAsync(user, Roles.Member);
                await userManager.AddToRoleAsync(user, Roles.Approver);
            }
            return allSucceeded;
        }
    }
}
