using CleanArchitecture.CorePackages.Persistance.Repositories;

public class EmailAuthenticator:Entity<int>
{
    public int UserId { get; set; }
    public string? ActivationKey { get; set; }
    public bool IsVerified { get; set; } 
    public virtual User User { get; set; } = null!;

    public EmailAuthenticator()
    {
        
    }

    public EmailAuthenticator(int userId, bool isVerified)
    {
        UserId = userId;
        IsVerified = isVerified;
    }

    public EmailAuthenticator(int id, int userId, bool isVerified):base(id)
    {
        // Id = id;
        UserId = userId;
        IsVerified = isVerified;
    }
}