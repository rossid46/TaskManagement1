using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Api.Models;
using TaskManagement.Api.Settings;

[Route("api")]
[ApiController]
public class ApiAuthJwtController : ControllerBase
{
    private readonly string JWT_ISSUER;
    private readonly string JWT_AUDIENCE;
    private readonly byte[] JWT_SECRET_KEY;

    private readonly ILogger<ApiAuthJwtController> _logger;

    private const string JWT_TOKEN_ID = "id";
    private const int JWT_EXPIRATION_MINUTES = 15;

    public ApiAuthJwtController(
        IOptions<AppJwtSettings> settings,
        ILogger<ApiAuthJwtController> logger) : base()
    {
        JWT_ISSUER = settings.Value.Issuer;
        JWT_AUDIENCE = settings.Value.Audience;
        JWT_SECRET_KEY = Encoding.UTF8.GetBytes(settings.Value.SecretKey);
        _logger = logger;
    }

    [Route("auth")]
    [HttpPost]
    [AllowAnonymous]
    public IActionResult AuthPost(
        [FromBody] AuthJwtRegistration user)
    {
        DateTime expiration = DateTime.UtcNow.AddMinutes(JWT_EXPIRATION_MINUTES);

        // configure and send final token.

        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new (JWT_TOKEN_ID, Guid.NewGuid().ToString()),
                new (JwtRegisteredClaimNames.Sub, user.User),
                new (JwtRegisteredClaimNames.Email, user.User),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }),
            SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(JWT_SECRET_KEY), SecurityAlgorithms.HmacSha256Signature),
            Expires = expiration,
            Issuer = JWT_ISSUER,
            Audience = JWT_AUDIENCE,
        };

        var handler = new JwtSecurityTokenHandler();
        SecurityToken token = handler.CreateToken(descriptor);

        _logger.LogTrace($"Created new JWT token. (expiration = {expiration:G})");

        string tokenString = handler.WriteToken(token);
        return Ok(tokenString);
    }
}