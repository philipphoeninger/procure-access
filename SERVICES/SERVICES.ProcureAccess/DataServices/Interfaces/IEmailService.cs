namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IEmailService
{
    Task<Task> SendEmailAsync(string to, string subject, string body);
}
