using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Models;
using System.Threading.Tasks;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("verify-admin")]
        public async Task<IActionResult> VerifyAdmin()
        {
            // Check if Admin role exists
            bool adminRoleExists = await _roleManager.RoleExistsAsync("Admin");

            // Check if admin user exists
            var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
            bool adminUserExists = adminUser != null;

            // Check if admin user is in Admin role
            bool adminInRole = adminUserExists && await _userManager.IsInRoleAsync(adminUser, "Admin");

            // Get all users in Admin role
            var usersInAdminRole = new List<object>();
            if (adminRoleExists)
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                usersInAdminRole = adminUsers.Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.UserName
                }).ToList<object>();
            }

            return Ok(new
            {
                AdminRoleExists = adminRoleExists,
                AdminUserExists = adminUserExists,
                AdminInRole = adminInRole,
                UsersInAdminRole = usersInAdminRole
            });
        }
    }
}
