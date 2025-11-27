namespace API.ProcureAccess.Controllers.Identity;

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
        IOptions<JWTSettings> jwtSettings)
    {
        User? user = await userManager.FindByEmailAsync(loginRequest.Email);
        if (user != null && await userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            SymmetricSecurityKey signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key));
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    signInKey,
                    SecurityAlgorithms.HmacSha256Signature
                    )
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);
            return Results.Ok(new { token, username = user.UserName });
        }
        else return Results.BadRequest(new { message = "Username or password is incorrect." });
    }

    [AllowAnonymous]
    private static async Task<IResult> SignUpAsync(
        UserManager<User> userManager,
        [FromBody] RegistrationRequest registerRequest)
    {
        User user = new User()
        {
            Email = registerRequest.Email,
            UserName = registerRequest.UserName
        };
        IdentityResult result = await userManager.CreateAsync(user, registerRequest.Password);

        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }
}
