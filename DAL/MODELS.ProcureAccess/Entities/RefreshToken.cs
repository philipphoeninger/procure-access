namespace MODELS.ProcureAccess.Entities;

[Table("RefreshTokens", Schema = "dbo")]
[EntityTypeConfiguration(typeof(RefreshTokenConfiguration))]
public class RefreshToken
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Token { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsRevoked { get; set; }
}
