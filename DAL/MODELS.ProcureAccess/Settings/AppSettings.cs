namespace MODELS.ProcureAccess.Settings;

public class JWTSettings
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}

public class AppSettings
{
    public JWTSettings JWTSettings { get; set; }
}
