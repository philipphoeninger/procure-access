// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Platform.Contracts.Email;
using IEmailSender = ProcureAccess.Modules.Platform.Contracts.Email.IEmailSender;

namespace SERVICES.ProcureAccess.DataServices;

/// <summary>
/// Legacy account flows (replaced by S5-12, S5-13 and removed by S5-10). Since S5-11 (#302, owner
/// decision 3a) the confirmation and reset mails go to the user's own address through Platform's
/// <see cref="IEmailSender"/> with the <c>email.*</c> texts; a failed send is logged and the request
/// answers as before (the account stays; the reset answers the same for every address).
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDBContext _db;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeSpan _tokenLifespan;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ApplicationDBContext db,
        IEmailSender emailSender,
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor,
        IOptions<DataProtectionTokenProviderOptions> tokenOptions,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _emailSender = emailSender;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _tokenLifespan = tokenOptions.Value.TokenLifespan;
        _logger = logger;
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

        try
        {
            await SendConfirmationEmailAsync(user.Email!);
        }
        catch (EmailDeliveryException)
        {
            // No address in the log (PRV-11); the account stays, S5-12 adds the resend.
            _logger.LogWarning("The confirmation e-mail of a new account could not be sent.");
        }

        return IdentityResult.Success;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !user.EmailConfirmed)
            return; // prevent user enumeration

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = Uri.EscapeDataString(token);

        var link = AppLink($"/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}");
        var validHours = (int)Math.Ceiling(_tokenLifespan.TotalHours);

        try
        {
            await _emailSender.SendAsync(new PasswordResetEmail(user.Email!, link, validHours));
        }
        catch (EmailDeliveryException)
        {
            // Same answer for every address (no user enumeration); no address in the log (PRV-11).
            _logger.LogWarning("A password reset e-mail could not be sent.");
        }
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

        var confirmationLink = AppLink($"/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={encodedToken}");

        await _emailSender.SendAsync(new AddressConfirmationEmail(user.Email!, confirmationLink));
    }

    /// <summary>A link into the client: <c>App:BaseUrl</c> (absolute) plus the path and query.</summary>
    private Uri AppLink(string pathAndQuery)
    {
        var baseUrl = _config["App:BaseUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var root))
        {
            throw new InvalidOperationException("App:BaseUrl must be the absolute address of the client.");
        }

        return new Uri(root.GetLeftPart(UriPartial.Authority) + root.AbsolutePath.TrimEnd('/') + pathAndQuery);
    }
}
