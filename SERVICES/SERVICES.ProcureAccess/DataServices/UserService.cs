namespace SERVICES.ProcureAccess.DataServices;

using Claim = System.Security.Claims.Claim;
using ClaimsIdentity = System.Security.Claims.ClaimsIdentity;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDBContext _db;
    private readonly IMapper _mapper;
    private readonly JWTSettings _jwt;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ApplicationDBContext db,
        IMapper mapper,
        IOptions<JWTSettings> jwt,
        IEmailService emailService,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _mapper = mapper;
        _jwt = jwt.Value;
        _emailService = emailService;
        _config = config;
    }

    // SIGN IN (with lockout support)
    public async Task<AuthResponseDto?> SignInAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || user.IsDeleted)
            return null; //gate

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            return null; //gate

        if (!user.EmailConfirmed)
            throw new Exception("Email not confirmed");

        return await GenerateAuthResponse(user);
    }

    // SIGN UP
    public async Task<IdentityResult> SignUpAsync(RegistrationRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            UserName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return result; //gate

        await _userManager.AddToRoleAsync(user, Roles.Member);

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var encodedToken = Uri.EscapeDataString(token);

        var confirmationLink = $"{_config["App:BaseUrl"]}/confirm-email?userId={user.Id}&token={encodedToken}";

        // Send confirmation mail
        var html = $"""
        <h2>Welcome to ProcureAccess</h2>
        <p>Please confirm your email address:</p>

        <p>
            <a href="{confirmationLink}" 
            style="padding:10px 20px;background:#4CAF50;color:white;text-decoration:none;">
            Confirm Email
            </a>
        </p>

        <p>If you didn’t request this, ignore this email.</p>
        """;

        await _emailService.SendEmailAsync(user.Email!, "Confirm your email", html);

        return IdentityResult.Success;
    }

    // REFRESH TOKEN
    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            return null; //gate

        // revoke old token (rotation)
        token.IsRevoked = true;

        await _db.SaveChangesAsync();

        return await GenerateAuthResponse(token.User);
    }

    // PRIVATE: Generate Tokens
    private async Task<AuthResponseDto> GenerateAuthResponse(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var permissions = await (
            from role in _db.Roles
            join claim in _db.RoleClaims on role.Id equals claim.RoleId
            where roles.Contains(role.Name!) &&
                  claim.ClaimType == CustomClaimTypes.Permission
            select claim.ClaimValue
        ).Distinct().ToArrayAsync();

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r)),
            ..permissions.Select(p => new Claim(CustomClaimTypes.Permission, p))
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(handler.CreateToken(tokenDescriptor));

        // create refresh token
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            User = _mapper.Map<UserDto>(user)
        };
    }

    public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found"
            }); //gate

        var decodedToken = Uri.UnescapeDataString(token);

        return await _userManager.ConfirmEmailAsync(user, decodedToken);
    }

    public async Task SendConfirmationEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || user.EmailConfirmed)
            return; //gate

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var encodedToken = Uri.EscapeDataString(token);

        var link = $"{_config["App:BaseUrl"]}/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Confirm your email",
            link);
    }
}
