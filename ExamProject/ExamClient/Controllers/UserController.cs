using ExamProject.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExamClient.Controllers
{
    public class UserController : Controller
    {
        string link = "https://localhost:5000/Auth";

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(ILogger<HomeController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(IFormCollection f)
        {
            string email = f["email"];
            string password = f["password"];
            string token = await LoginAsync(email, password);
            var userRole = HttpContext.Session.GetString("UserRole");
            string msg = "";
            if (token == null)
            {
                msg = "Wrong username or password";
                ViewBag.Message = msg;
                return View();
            }
            ViewBag.Message = msg;
            if (userRole.Equals("admin"))
            {
                return RedirectToAction("Index", "Question");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        private async Task<string> LoginAsync(string email, string password)
        {
            var link = $"https://localhost:5000/api/Auth?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        return null;
                    }
                    else
                    {
                        var content = await res.Content.ReadAsStringAsync();
                        var tokenObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                        var token = tokenObj["token"];
                        DecodeTokenAndStoreInSession(token);
                        if (!string.IsNullOrEmpty(token))
                        {
                            HttpContext.Session.SetString("AccessToken", token);
                        }

                        return token;
                    }
                }
            }
        }

        private void DecodeTokenAndStoreInSession(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jsonToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var userName = jsonToken.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
            var userRole = jsonToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

            // Lưu thông tin vào session
            _httpContextAccessor.HttpContext.Session.SetString("UserId", userId);
            _httpContextAccessor.HttpContext.Session.SetString("UserName", userName);
            _httpContextAccessor.HttpContext.Session.SetString("UserRole", userRole);
        }

    }
}
