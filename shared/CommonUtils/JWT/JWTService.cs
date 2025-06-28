using Contracts.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CommonUtils.JWT;

public class JWTService : IJWTService
{
    public UserToken GetUserToken(JWTUserModel user)
    {
        var JwtKey = "d47c9512f6e7de79efb7ea748d322ad9ada04290e752283e04400f27aeade5cbd5c49c8a5dce653cac523a2762656e6e3ee63bba1666df6ea0bb1276cf3b65248a37a09b2f40509a7d6e9ec60a3b7e3c68ca2d9eb60c41a36d3a42a523b44da2eb250d8706cccc4344b416dbb75131cccb64a49a30e564b0cd65de27df76d71312adcfc76decf23458ddcbd83dfcb1b0f60395a3fad3f6514f02df57ab2044422e20c04b886a996f08f885a5328c5324e6f1dd8e9bfa0b490516ccfd741eff5af5f0d7ba2372f332d0dc2bc47d0dbb1f87037de5bf1a578d07c90e0c811f137b5e8ca4f3bb2967d880c7363d17c6611fbfa6518feb82ba0249e0b40391f65e19";
        var JwtIssuer = "ArtPlatform";
        var JwtAudience = "ArtPlatform";
        var JwtDuration = 300; // 5 hours

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToUpper())
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(JwtDuration),
            signingCredentials: credentials
        );

        return new UserToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}