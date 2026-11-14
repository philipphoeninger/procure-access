// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

/// <summary>
/// Legacy account read (<c>GET /api/me</c>, removed with the users' move into IdentityAccess, M3.1). The
/// legacy sign-up and address confirmation went with S5-12b, the legacy password reset with S5-13b
/// (owner decision S5-13 4 (a)); their replacements are the endpoints under <c>/api/v1/auth</c>.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
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
}
