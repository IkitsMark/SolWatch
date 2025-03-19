using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SolarWatch.Service.Authentication;

namespace SolarWatch.Services.Authentication;
//Service to create a JWT token for the user
public class TokenService : ITokenService
{
    private const int ExpirationMinutes = 30;
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    //Constructor for the TokenService, takes configuration and tokenHandler as parameters
    public TokenService(IConfiguration configuration, JwtSecurityTokenHandler tokenHandler)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _tokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));
    }

    public string CreateToken(IdentityUser user, string role)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        var token = CreateJwtToken(
            CreateClaims(user, role),
            CreateSigningCredentials(),
            expiration
        );
        return _tokenHandler.WriteToken(token);
    }
    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
        DateTime expiration)
    {
        var issuer = _configuration["Jwt:ValidIssuer"] 
            ?? throw new InvalidOperationException("JWT Issuer is missing in configuration.");
        var audience = _configuration["Jwt:ValidAudience"] 
            ?? throw new InvalidOperationException("JWT Audience is missing in configuration.");

        return new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );
    }

    private List<Claim> CreateClaims(IdentityUser user, string? role)
    {

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, "TokenForTheApiWithAuth"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow)
                .ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email)
            };
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

        return claims;
    }
    private SigningCredentials CreateSigningCredentials()
    {
        var key = _configuration["Jwt:IssuerSigningKey"];

            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("JWT signing key is missing in configuration.");
            }

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        return new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
    }
}