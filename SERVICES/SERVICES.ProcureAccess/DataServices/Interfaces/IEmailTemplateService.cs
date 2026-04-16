namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IEmailTemplateService
{
    Task<string> RenderAsync(string templateName, Dictionary<string, string> values);
}
