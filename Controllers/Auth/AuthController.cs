using BCrypt.Net;
using LibraryApi.Contracts.Auth;
using LibraryApi.Infrastructure.Auth;
using LibraryApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(AppDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
       
        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Email.ToLower() == email, ct);

        if (user is null || !user.IsActive)
            return Unauthorized("Invalid credentials.");

        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok)
            return Unauthorized("Invalid credentials.");

        var (token, expiresAtUtc) = _jwt.CreateToken(user);

        return Ok(new LoginResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc
        });
    }

    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
    return Ok("Authorized!");
    }
    
}