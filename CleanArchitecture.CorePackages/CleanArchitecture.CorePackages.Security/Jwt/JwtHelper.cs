
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtHelper : ITokenHelper
{
    public IConfiguration Configuration { get; }
    private readonly TokenOptions _tokenOptions;
    private DateTime _accessTokenExpiration;

    public JwtHelper(IConfiguration configuration)
    {
        Configuration = configuration;
        const string configurationSection="TokenOptions";
        _tokenOptions = 
            Configuration.GetSection(configurationSection).Get<TokenOptions>()
             ?? throw new NullReferenceException($"\"{configurationSection}\" section is cannot be null");
    }
    public RefreshToken CreateRefreshToken(User user, string ipAddress)
    {
        RefreshToken refreshToken = new ()
        {
            UserId = user.Id,
            Token = RandomRefreshToken(),
            Expires = DateTime.Now.AddDays(3),
            CreatedByIp = ipAddress
        };
        return refreshToken;
    }

    public AccessToken CreateToken(User user, List<OperationClaim> operationClaims)
    {
        _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
        SecurityKey securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
        SigningCredentials signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
        JwtSecurityToken jwtSecurityToken = CreateJwtSecurityToken(_tokenOptions, user, signingCredentials, operationClaims);
        JwtSecurityTokenHandler jwtSecurityTokenHandler = new ();
        string? token = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);

        return new AccessToken { Token = token, Expiration = _accessTokenExpiration };
    }

    public JwtSecurityToken CreateJwtSecurityToken(TokenOptions tokenOptions, User user, SigningCredentials signingCredentials, List<OperationClaim> operationClaims)
    {
        JwtSecurityToken jwt = new (
            issuer: tokenOptions.Issuer,
            audience: tokenOptions.Audience,
            expires: _accessTokenExpiration,
            notBefore: DateTime.Now,
            claims: SetClaims(user, operationClaims),
            signingCredentials: signingCredentials
        );
        return jwt;
    }

    private IEnumerable<Claim> SetClaims(User user, IList<OperationClaim> operationClaims)
    {
        List<Claim> claims = new ();
        claims.AddNameIdentifier(user.Id.ToString());
        claims.AddEmail(user.Email);
        claims.AddName($"{user.FirstName} {user.LastName}");
        claims.AddRoles(operationClaims.Select(c => c.Name).ToArray());
        return claims;
    }

    private string RandomRefreshToken()
    {
        byte[] numberByte = new byte[32];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(numberByte);
        return Convert.ToBase64String(numberByte);
    }
}