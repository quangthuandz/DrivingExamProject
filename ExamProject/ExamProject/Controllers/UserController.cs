using ExamProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ExamProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DrivingExamQuizContext _context;

        public UserController(DrivingExamQuizContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(string email, string password)
        {
            var data = _context.Users.Where(x => x.Email == email && x.Password == password).FirstOrDefault();
            if (data == null)
            {
                return BadRequest("Wrong username or password");
            }
            return Ok(data);
        }

        [HttpGet("all")]
        [Authorize]
        public IActionResult GetAllUser()
        {
            var data = _context.Users.ToList();
            return Ok(data);
        }

        [HttpGet("allExam")]
        [Authorize]
        public IActionResult GetAllExam()
        {
            var data = _context.Exams.ToList();
            return Ok(data);
        }
    }
}
