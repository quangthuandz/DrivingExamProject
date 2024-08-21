using ExamProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamQuestionController : ControllerBase
    {
        private DrivingExamQuizContext _context;
        public ExamQuestionController(DrivingExamQuizContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Post(ExamQuestion eq)
        {
            _context.Add(eq);
            _context.SaveChanges();
            return Ok("Insert Success");
        }

        [HttpGet("GetQuestionResultByExamId")]
        public IActionResult GetQuestionResultByExamId(int examId)
        {
            var resultQuestion = _context.ExamQuestions.Where(x => x.ExamId == examId).ToList();
            return Ok(resultQuestion);
        }
    }
}
