using EWP_API_WEB_APP.Data;
using EWP_API_WEB_APP.Models.API.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EWP_API_WEB_APP.Models.Data;
using EWP_API_WEB_APP.Models.API.Requests;

namespace EWP_API_WEB_APP.Controllers.API
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(ApplicationDbContext context, UserManager<Users> userManager,
            SignInManager<Users> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // POST: Auth/logout
        [HttpPost]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        // POST: Auth/login
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(LoginRequest loginRequest)
        {
            LoginResponse loginResponse = new LoginResponse();

            var user = await _userManager.FindByEmailAsync(loginRequest.email);

            if (user != null)
            {
                PasswordVerificationResult passWorks = new PasswordHasher<Users>().VerifyHashedPassword(null, user.PasswordHash, loginRequest.password);
                if (passWorks.Equals(PasswordVerificationResult.Success))
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    loginResponse.User = user;
                    loginResponse.LoginSuccess = true;
                }
            }

            return Ok(loginResponse);
        }

        // POST: Auth/register
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(RegisterRequest registerRequest)
        {
            RegisterResponse registerResponse = new RegisterResponse();
            var roleName = "Client";

            if (registerRequest.accountype.Equals(2))
                roleName = "Enterprise";

            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var role = new IdentityRole(roleName);
                await _roleManager.CreateAsync(role);
            }

            var user = new Users
            {
                UserName = registerRequest.userName,
                Email = registerRequest.email,
                Name = registerRequest.name,
                CreationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                PhoneNumber = registerRequest.cellphone,
                AccessLevel = registerRequest.accountype,
                Status = (int) UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerRequest.password);

            if (result.Succeeded)
            {
                registerResponse.result = true;
                await _userManager.AddToRoleAsync(user, roleName);
                await _userManager.UpdateAsync(user);
            }

            return Ok(registerResponse);
        }
    }
}
