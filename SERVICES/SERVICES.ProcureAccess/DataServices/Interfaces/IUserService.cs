namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IUserService
{
    Task<AuthResponseDto?> SignInAsync(LoginRequest request);
    Task<IdentityResult> SignUpAsync(RegistrationRequest request);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
    Task SendConfirmationEmailAsync(string email);
}
