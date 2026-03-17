namespace API.ProcureAccess.Controllers.Models;

public class RegistrationRequest : IRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
