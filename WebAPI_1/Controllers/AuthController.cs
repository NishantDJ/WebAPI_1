using Microsoft.AspNetCore.Mvc;
using WebAPI_1.Model;
using WebAPI_1.Services;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        var token = _authService.Login(dto.Username, dto.Password);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        _authService.Register(dto);
        return Ok("User registered successfully");
    }

}
