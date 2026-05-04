namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IUserService
{
    Task<AuthResponseDto?> SignInAsync(LoginRequest request);
    Task<IdentityResult> SignUpAsync(RegistrationRequest request);
    Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
    Task SendConfirmationEmailAsync(string email);
    Task RequestPasswordResetAsync(string email);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<UserDto?> GetCurrentUser();
}
