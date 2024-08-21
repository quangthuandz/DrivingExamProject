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

        /*[HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO user, string role)
        {
            var userExist = await userManager.FindByEmailAsync(user.Email);
            if (userExist != null)
            {
                return BadRequest("email existed!");
            }
            if (await roleManager.RoleExistsAsync(role))
            {
                uzer newUser = new()
                {
                    Email = user.Email,
                    UserName = user.Name
                };
                var register = await userManager.CreateAsync(newUser, user.Password);
                if (!register.Succeeded)
                {
                    var error = string.Join(", \n", register.Errors.Select(e => e.Description));
                    return BadRequest("register that bai: " + error);
                }
                await userManager.AddToRoleAsync(newUser, role);
                return Ok("regsiter thanh cong");
            }
            else
            {
                return BadRequest("invalid role");
            }
        }*/

     

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                //var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                if (user.Password.Equals(password))
                {
                    var jwtToken = GetTokenAsync(user);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(await jwtToken),
                        expiresAt = (await jwtToken).ValidTo
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
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("isADmin", "dsd")
                };
            /*var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                
            }*/
            authClaims.Add(new Claim("role", user.Role));
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var signAlg = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                signingCredentials: signAlg,
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: authClaims,
                expires: DateTime.Now.AddMinutes(30));

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
