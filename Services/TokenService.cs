using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.Models;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Services;

public static  class TokenService
{

    public static string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Settings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity( new []
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

       var token = tokenHandler.CreateToken(tokenDescriptor);
       return tokenHandler.WriteToken(token);
    }


    public static string GenerateToken(IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Settings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

       var token = tokenHandler.CreateToken(tokenDescriptor);
       return tokenHandler.WriteToken(token);
    }

    public static string GenerateRefreshToken()
    {
        var randomNumber = new Byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidatorParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Secret)),
            ValidateLifetime = false
        };

        var tokenHandler =  new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidatorParameters, out var securityToken);

        if(securityToken is not JwtSecurityToken jwtSecurityToken || 
        !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
        StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("InvalidToken");

        return principal;
    }

    private static List<(string, string)> _refreshTokens = new();

    public static void SaveRefreshToken(string userName, string refreshToken)
    {
        _refreshTokens.Add(new (userName, refreshToken));
    }

        public static string GetRefreshToken(string userName)
    {
        return _refreshTokens.FirstOrDefault(x => x.Item1 == userName).Item2;
    }

        public static void DeleteRefreshToken(string userName, string refreshToken)
    {
       var item = _refreshTokens.FirstOrDefault(x => x.Item1 == userName && x.Item2 == refreshToken);
       _refreshTokens.Remove(item);
    }
}