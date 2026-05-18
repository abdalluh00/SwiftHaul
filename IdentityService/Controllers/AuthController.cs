using IdentityService.Application.DTOs;
using IdentityService.Common.Helpers;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Check email already exists
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email.ToLower());

            if (emailExists)
                return BadRequest("Email is already registered.");

            // Create user
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Role = (UserRole)request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (token, expiresAt) = _jwtHelper.GenerateToken(user);

            return Ok(new AuthResponse(
                token,
                user.FullName,
                user.Email,
                user.Role.ToString(),
                expiresAt
            ));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            if (!user.IsActive)
                return Unauthorized("Your account is deactivated.");

            var (token, expiresAt) = _jwtHelper.GenerateToken(user);

            return Ok(new AuthResponse(
                token,
                user.FullName,
                user.Email,
                user.Role.ToString(),
                expiresAt
            ));
        }

    }


}
