namespace API.ProcureAccess.Extensions;

public static class AuthorizationExtensions
{
    public static async Task AddRolesAndPermissions(this WebApplication app, RoleManager<IdentityRole> roleManager)
    {
        // set approver permissions
        string[] roles = { Roles.Admin, Roles.Approver };
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                await roleManager.CreateAsync(role = new IdentityRole(roleName));
                await roleManager.AddClaimAsync(
                    role,
                    new Claim(CustomClaimTypes.Permission, Permissions.ObjectsApprove));
            }
        }

        // TODO: set member permissions
        var memberRole = await roleManager.FindByNameAsync(Roles.Member);
        if (memberRole is null)
        {
            await roleManager.CreateAsync(memberRole = new IdentityRole(Roles.Member));
            // ...
        }
    }
}
