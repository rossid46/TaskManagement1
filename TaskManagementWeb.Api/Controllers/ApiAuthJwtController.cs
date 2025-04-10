using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.DataAccess.Interfaces;
using TaskManagement.Models;
using TaskManagement.Utility;


[Route("api")]
[ApiController]
public class ApiAuthJwtController : ControllerBase
{
    private readonly string JWT_ISSUER;
    private readonly string JWT_AUDIENCE;
    private readonly byte[] JWT_SECRET_KEY;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AuthJwtRegistration> _validator;
    private readonly ILogger<ApiAuthJwtController> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    private const string JWT_TOKEN_ID = "id";
    private const int JWT_EXPIRATION_MINUTES = 15;

    public ApiAuthJwtController(
        IUnitOfWork unitOfWork,
        IOptions<AppJwtSettings> settings,
        IValidator<AuthJwtRegistration> validator,
        ILogger<ApiAuthJwtController> logger,
        UserManager<IdentityUser> userManager) : base()
    {
        _unitOfWork = unitOfWork;
        JWT_ISSUER = settings.Value.Issuer;
        JWT_AUDIENCE = settings.Value.Audience;
        JWT_SECRET_KEY = Encoding.UTF8.GetBytes(settings.Value.SecretKey);
        _validator = validator;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> AuthPost(
        [FromBody] AuthJwtRegistration userAuth)
    {
        var result = await _validator.ValidateAsync(userAuth);

        if (!result.IsValid)
        {
            return BadRequest(result.Errors);
        }
        var user = await _userManager.FindByEmailAsync(userAuth.User);
        if (!await _userManager.CheckPasswordAsync(user, userAuth.Password))
        {
            _logger.LogError("Wrong credentials.");
            return Unauthorized();
        }
        DateTime expiration = DateTime.Now.AddMinutes(JWT_EXPIRATION_MINUTES);

        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                   new (JWT_TOKEN_ID, Guid.NewGuid().ToString()),
                   new (JwtRegisteredClaimNames.Sub, user.Id),
                   new (JwtRegisteredClaimNames.Email, user.Email),
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

        _logger.LogInformation($"Created new JWT token. (expiration = {expiration:G})");

        string tokenString = handler.WriteToken(token);
        return Ok(tokenString);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<IdentityError>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterPost(
    [FromBody] AuthJwtRegistration request)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return BadRequest(result.Errors);
        }

        string normalizedEmail = _userManager.NormalizeEmail(request.User);
        ApplicationUser? appUser = _unitOfWork.ApplicationUser.Get(u => u.NormalizedEmail == normalizedEmail);

        if (appUser != null)
        {
            // alert that the user already exists.
            _logger.LogInformation($"Attempted to register an existing user. (user = {request.User})");

            return Conflict("another account is already registered by that email. sorry.");
        }

        appUser = new ApplicationUser()
        {
            Name = request.User!,
        };

        await _userManager.SetUserNameAsync(appUser, request.User!);
        await _userManager.SetEmailAsync(appUser, request.User!);
        IdentityResult resultCreate = await _userManager.CreateAsync(appUser, request.Password!);

        if (!resultCreate.Succeeded)
        {
            return BadRequest(resultCreate.Errors);
        }

        await _userManager.AddToRoleAsync(appUser, SD.Role_User);
        _logger.LogInformation($"Add user. (user = {appUser.Email}, role = {SD.Role_User})");

        return Ok();
    }
}
