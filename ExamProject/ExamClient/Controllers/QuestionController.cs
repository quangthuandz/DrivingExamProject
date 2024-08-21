using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace ExamClient.Controllers
{
    public class QuestionController : Controller
    {
        public IActionResult Index()
        {
            string token = HttpContext.Session.GetString("AccessToken");
            bool check = IsUserAdmin(token);
            if(check == false)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not allowed to access this resource.");
            }
            ViewBag.AccessToken = token;
            return View();
        }

        private bool IsUserAdmin(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var roles = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            return roles.Contains("admin");
        }
    }
}
