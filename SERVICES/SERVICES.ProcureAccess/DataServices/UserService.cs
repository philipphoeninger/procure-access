// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDBContext _db;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ApplicationDBContext db,
        IEmailService emailService,
        IEmailTemplateService templateService,
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _emailService = emailService;
        _templateService = templateService;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        ClaimsPrincipal? claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal is null) return null; //gate
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null; //gate

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
            return null; //gate
        return user.ToDto();
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

        await SendConfirmationEmailAsync("ok@testing.lettermint.co"); //user.Email

        return IdentityResult.Success;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !user.EmailConfirmed)
            return; // prevent user enumeration

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = Uri.EscapeDataString(token);

        var link = $"{_config["App:BaseUrl"]}/reset-password?email={email}&token={encodedToken}";

        var html = await _templateService.RenderAsync("ResetPassword",
            new Dictionary<string, string>
            {
                { "link", link }
            });

        await _emailService.SendEmailAsync(
            email,
            "Reset your password",
            html);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Invalid request"
            });

        var decodedToken = Uri.UnescapeDataString(dto.Token);

        return await _userManager.ResetPasswordAsync(
            user,
            decodedToken,
            dto.NewPassword);
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

        var confirmationLink = $"{_config["App:BaseUrl"]}/confirm-email?userId={user.Id}&token={encodedToken}";

        var html = await _templateService.RenderAsync("ConfirmEmail",
            new Dictionary<string, string>
            {
                { "link", confirmationLink }
            });

        await _emailService.SendEmailAsync(user.Email!, "Confirm your email", html);
    }
}
