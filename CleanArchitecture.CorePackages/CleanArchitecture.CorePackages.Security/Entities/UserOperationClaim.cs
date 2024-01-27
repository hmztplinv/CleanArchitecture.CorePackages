using CleanArchitecture.CorePackages.Persistance.Repositories;

public class UserOperationClaim:Entity<int>
{
    public int UserId { get; set; }
    public int OperationClaimId { get; set; }

    public virtual User User { get; set; }
    public virtual OperationClaim OperationClaim { get; set; }
    
    // public UserOperationClaim()
    // {
    //     UserId = default;
    //     OperationClaimId = default;
    // }

    public UserOperationClaim(int userId, int operationClaimId)
    {
        UserId = userId;
        OperationClaimId = operationClaimId;
    }

    public UserOperationClaim(int id, int userId, int operationClaimId):base(id)
    {
        // Id = id;
        UserId = userId;
        OperationClaimId = operationClaimId;
    }
}