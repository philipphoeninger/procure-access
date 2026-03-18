namespace API.ProcureAccess.Controllers.Models;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }

    public string NewPassword { get; set; }
}
