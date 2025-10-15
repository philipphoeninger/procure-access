namespace DAL.ProcureAccess.Initialization;

public static class SampleData
{
    public static List<User> Users => new()
    {
        new() 
        { 
            UserName = "JohnDoe",
            Email = "john_doe@mail.mailtest",
            PasswordHash = "AQAAAAIAAYagAAAAEGed18XsPagWrAAPb3hyt7t66n9BA03w1ctadgdd09uFUiZ17BSUq1Wv3TRzAK94Jg==",
            SecurityStamp = "A3VLN7PDJ3LPKUGYHXOSG5RFQV3FDWFM",
            ConcurrencyStamp = "3411a4b2-b9a4-4554-b2c8-f90654dc0f0f",
        }
    };
}
