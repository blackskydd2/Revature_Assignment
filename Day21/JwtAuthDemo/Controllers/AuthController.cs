using Microsoft.AspNetCore.Mvc;
using JwtAuthDemo.Services;
using JwtAuthDemo.Models;

namespace JwtAuthDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest loginRequest)
    {
        if (loginRequest.Username == "sarah" && loginRequest.Password == "s@123")
        {
            var token = _tokenService.GenerateToken(loginRequest.Username, "Admin");
            return Ok(new { Token = token });
        }

        return Unauthorized();
    }
}