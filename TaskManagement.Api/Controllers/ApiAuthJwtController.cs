using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Api.Settings;
using TaskManagement.Api.Models;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

[Route("api")]
[ApiController]
public class ApiAuthJwtController : ControllerBase
{
    private readonly string JWT_ISSUER;
    private readonly string JWT_AUDIENCE;
    private readonly byte[] JWT_SECRET_KEY;
    
    private readonly IValidator<AuthJwtRegistration> _validator;
    private readonly ILogger<ApiAuthJwtController> _logger;

    private const string JWT_TOKEN_ID = "id";
    private const int JWT_EXPIRATION_MINUTES = 15;

    public ApiAuthJwtController(
        IOptions<AppJwtSettings> settings,
        IValidator<AuthJwtRegistration> validator,
        ILogger<ApiAuthJwtController> logger) : base()
    {
        JWT_ISSUER = settings.Value.Issuer;
        JWT_AUDIENCE = settings.Value.Audience;
        JWT_SECRET_KEY = Encoding.UTF8.GetBytes(settings.Value.SecretKey);
        _validator = validator;
        _logger = logger;
    }

    [Route("auth")]
    [HttpPost]
    [AllowAnonymous]
    public IActionResult AuthPost(
        [FromBody] AuthJwtRegistration user)
    {
        var result = _validator.Validate(user);

        if (result.IsValid)
        {
            return BadRequest(result.Errors);
        }

        DateTime expiration = DateTime.UtcNow.AddMinutes(JWT_EXPIRATION_MINUTES);

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
