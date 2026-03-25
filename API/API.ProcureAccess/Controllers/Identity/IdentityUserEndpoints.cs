namespace API.ProcureAccess.Controllers.Identity;

using Claim = System.Security.Claims.Claim;
using ClaimsIdentity = System.Security.Claims.ClaimsIdentity;

public static class IdentityUserEndpoints
{
    public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/signIn", SignInAsync);
        app.MapPost("/signUp", SignUpAsync);
        return app;
    }

    [AllowAnonymous]
    private static async Task<IResult> SignInAsync(
        UserManager<User> userManager,
        [FromBody] LoginRequest loginRequest,
        IOptions<JWTSettings> jwtSettings,
        ApplicationDBContext dbContext)
    {
        User? user = await userManager.FindByEmailAsync(loginRequest.Email);
        if (user != null && user.IsDeleted)
            return Results.BadRequest(Microsoft.AspNetCore.Identity.SignInResult.NotAllowed); //gate
        if (user != null && await userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            var roles = await userManager.GetRolesAsync(user);

            var permissions = await (
                from role in dbContext.Roles
                join claim in dbContext.RoleClaims on role.Id equals claim.RoleId
                where roles.Contains(role.Name!) &&
                    claim.ClaimType == CustomClaimTypes.Permission
                select claim.ClaimValue)
                    .Distinct()
                    .ToArrayAsync();

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                //new(ClaimTypes.NameIdentifier, user.Id.ToString()), //old claim
                new(JwtRegisteredClaimNames.Email, user.Email!),
                ..roles.Select(r => new Claim(ClaimTypes.Role, r)),
                ..permissions.Select(p => new Claim(CustomClaimTypes.Permission, p))
            ];

            SymmetricSecurityKey signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key));
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    signInKey,
                    SecurityAlgorithms.HmacSha256Signature
                    )
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);
            return Results.Ok(new { token, email = user.Email, uiCustomization = user.UICustomization });
        }
        else return Results.BadRequest(new { message = "Email or password is incorrect." });
    }

    [AllowAnonymous]
    private static async Task<IResult> SignUpAsync(
        UserManager<User> userManager,
        [FromBody] RegistrationRequest registerRequest)
    {
        User user = new User()
        {
            Email = registerRequest.Email,
            UserName = registerRequest.Email
        };
        IdentityResult result = await userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded) return Results.BadRequest(result); //gate

        IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, Roles.Member);
        if (!addToRoleResult.Succeeded) return Results.BadRequest(addToRoleResult); //gate

        return Results.Ok(result);
    }
}
