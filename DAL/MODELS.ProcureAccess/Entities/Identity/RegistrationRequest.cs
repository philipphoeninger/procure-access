namespace MODELS.ProcureAccess.Entities.Identity;

public class RegistrationRequest : IRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
