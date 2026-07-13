using Microsoft.AspNetCore.Mvc;
using NovaEra.API.Data;
using NovaEra.API.DTOs;
using NovaEra.API.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NovaEra.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    
    public AuthController(
    ApplicationDbContext context,
    IConfiguration configuration)
    {
    _context = context;
    _configuration = configuration;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (_context.Users.Any(x => x.Email == request.Email))
            return BadRequest("Email already exists");


        using var hmac = new HMACSHA512();


        var user = new User
        {
            Name = request.Name,
            Email = request.Email,

            PasswordSalt = Convert.ToBase64String(hmac.Key),

            PasswordHash = Convert.ToBase64String(
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(request.Password)
                )
            ),

            Role = "Customer"
        };


        _context.Users.Add(user);

        await _context.SaveChangesAsync();


        return Ok(user);
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = _context.Users
            .FirstOrDefault(x => x.Email == request.Email);


        if (user == null)
            return Unauthorized("Invalid Email");


        using var hmac = new HMACSHA512(
            Convert.FromBase64String(user.PasswordSalt)
        );


        var computedHash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(request.Password)
        );


        if (computedHash.SequenceEqual(
            Convert.FromBase64String(user.PasswordHash)) == false)
        {
            return Unauthorized("Invalid Password");
        }

var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Role, user.Role)
};


var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(
        _configuration["Jwt:Key"]!
    )
);


var creds = new SigningCredentials(
    key,
    SecurityAlgorithms.HmacSha256
);


var token = new JwtSecurityToken(
    issuer: _configuration["Jwt:Issuer"],
    audience: _configuration["Jwt:Audience"],
    claims: claims,
    expires: DateTime.Now.AddHours(3),
    signingCredentials: creds
);


return Ok(new
{
    token = new JwtSecurityTokenHandler().WriteToken(token)
});
    }
}