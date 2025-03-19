using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Contacts;
using SolarWatch.Service.Authentication;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authenticationService;
    private readonly ILogger<AuthController> _logger;
    public AuthController(IAuthService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var result = await _authenticationService.RegisterAsync(request.Email, request.Username, request.Password, request.Role);

            if (!result.Success)
            {
                AddErrors(result);
                return BadRequest(ModelState);
            }

            return CreatedAtAction(nameof(Register), new RegistrationResponse(result.Email, result.UserName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration.");
            return StatusCode(500, "Internal server error");
        }
    }

    private void AddErrors(AuthResult result)
    {
        foreach (var error in result.ErrorMessages)
        {
            ModelState.AddModelError(error.Key, error.Value);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var result = await _authenticationService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                AddErrors(result);
                return BadRequest(ModelState);
            }

            return Ok(new AuthResponse(result.Email, result.UserName, result.Token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during authentication.");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("isadmin"), Authorize(Roles = "User, Admin")]
    public async Task<ActionResult<bool>> SendBackRole()
    {
        try
        {
            var userName = HttpContext.User.Identity.Name;
            var isAdmin = await _authenticationService.IsAdmin(userName);

            return Ok(isAdmin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking admin role.");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("isexpired"), Authorize(Roles = "User, Admin")]
    public ActionResult<bool> IsExpired()
    {
        return Ok(false);
    }
}