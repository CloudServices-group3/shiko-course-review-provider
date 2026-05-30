using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Shiko.CourseReviewProvider.Api.IntegrationTests.TestInfrastructure;

public static class JwtTokenFactory
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "test-signing-key-for-integration-tests-123456789";

    public static string CreateUserToken(string userId = "test-user-id")
    {
        return CreateToken(userId);
    }

    private static string CreateToken(string userId)
    {
        var claims = new List<Claim>
        {
            new("userId", userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.Sub, userId)
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(SigningKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}