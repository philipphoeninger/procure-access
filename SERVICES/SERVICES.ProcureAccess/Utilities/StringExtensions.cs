namespace SERVICES.ProcureAccess.Utilities;

public static class StringExtensions
{
    public static string RemoveControllerSuffix(this string original)
       => original.Replace("Controller", "", StringComparison.OrdinalIgnoreCase);
    public static string RemoveAsyncSuffix(this string original)
        => original.Replace("Async", "", StringComparison.OrdinalIgnoreCase);
    public static string RemovePageModelSuffix(this string original)
        => original.Replace("PageModel", "", StringComparison.OrdinalIgnoreCase);
}
