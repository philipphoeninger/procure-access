namespace DAL.ProcureAccess.Initialization;

public static class SampleData
{
    public static List<User> Users => new()
    {
        new() 
        {
            UserName = "hoeninger@werte.it",
            Email = "hoeninger@werte.it",
            UICustomization = new(),
            PasswordHash = "AQAAAAIAAYagAAAAEIXWOPxH++T/W4r8ji4Kl9oOMkl4YhnN+mpMYFOdKhGugALVkLTBv5N+tVf9Zz2YUA==",
            SecurityStamp = "JFL3OXKFWI23KPP5DGEJ62HFZ4T2TRMR",
            ConcurrencyStamp = "b01be0de-8be0-45c3-9fda-152bfb9804d9",
            EmailConfirmed = true
        }
    };
}
