using ExamProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace IdentityTest1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private IConfiguration configuration;

        public AuthController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (user.Password.Equals(password))
                {
                    var jwtToken = GetTokenAsync(user);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(await jwtToken)
                    });
                }
                else
                {
                    return Unauthorized("Invalid login attempt");
                }
            }
            return Unauthorized("Invalid login attempt");
        }

        private async Task<JwtSecurityToken> GetTokenAsync(User user)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name), 
                    new Claim(ClaimTypes.Role, user.Role), 
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var signAlg = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: authClaims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: signAlg);

            return token;
        }


        [HttpGet("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);

            await signInManager.SignOutAsync();
            await userManager.UpdateSecurityStampAsync(user);
            return Ok("Logout success");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/Unauthorized")]
        public IActionResult unauthorized()
        {
            return Unauthorized();
        }
    }
}
