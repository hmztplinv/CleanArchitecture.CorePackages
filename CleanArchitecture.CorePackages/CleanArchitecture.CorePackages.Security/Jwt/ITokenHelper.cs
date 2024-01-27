public interface ITokenHelper
{
    AccessToken CreateToken(User user, List<OperationClaim> operationClaims);
    RefreshToken CreateRefreshToken(User user, string ipAddress);
}