using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceApi.Models;
using ECommerceApi.DTOs;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add user to "User" role
                await _userManager.AddToRoleAsync(user, "User");

                // Automatically log in the user and return token
                var token = await _tokenService.GenerateToken(user, _userManager);
                return Ok(new { token, message = "User created successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            // Try to find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                var token = await _tokenService.GenerateToken(user, _userManager);
                return Ok(new { token });
            }
            else
            {
                // Return more specific error information
                if (result.IsLockedOut)
                {
                    return StatusCode(423, new { message = "Account is locked out" });
                }
                else if (result.IsNotAllowed)
                {
                    return StatusCode(403, new { message = "Login not allowed" });
                }
                else if (result.RequiresTwoFactor)
                {
                    return StatusCode(428, new { message = "Two factor authentication required" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid password" });
                }
            }
        }

        [HttpGet("test")]
        [Authorize]  // First test with just [Authorize] to ensure basic auth works
        public async Task<IActionResult> Test()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                message = "You are authorized!",
                username = User.Identity.Name,
                roles = roles
            });
        }

        // Add an admin test endpoint
        [HttpGet("admin-check")]
        public async Task<IActionResult> AdminCheck()
        {
            // Check if admin exists and password is correct
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin123!@#";

            var user = await _userManager.FindByEmailAsync(adminEmail);
            if (user == null)
            {
                return NotFound(new { message = "Admin user not found in database" });
            }

            var isInAdminRole = await _userManager.IsInRoleAsync(user, "Admin");
            var passwordValid = await _userManager.CheckPasswordAsync(user, adminPassword);

            return Ok(new
            {
                userFound = true,
                isInAdminRole = isInAdminRole,
                passwordValid = passwordValid,
                emailConfirmed = user.EmailConfirmed
            });
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
