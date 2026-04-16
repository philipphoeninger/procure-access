namespace SERVICES.ProcureAccess.DataServices;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IWebHostEnvironment _env;

    public EmailTemplateService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> RenderAsync(string templateName, Dictionary<string, string> values)
    {
        var basePath = Path.Combine(_env.ContentRootPath, "EmailTemplates");

        var baseTemplate = await File.ReadAllTextAsync(
            Path.Combine(basePath, "BaseTemplate.html"));

        var content = await File.ReadAllTextAsync(
            Path.Combine(basePath, $"{templateName}.html"));

        foreach (var kv in values)
        {
            content = content.Replace($"{{{{{kv.Key}}}}}", kv.Value);
        }

        return baseTemplate.Replace("{{content}}", content);
    }
}
