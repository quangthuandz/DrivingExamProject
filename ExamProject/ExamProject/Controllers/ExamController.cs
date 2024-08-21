using ExamProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExamProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private DrivingExamQuizContext _context;
        public ExamController(DrivingExamQuizContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Post(Exam exam)
        {
            _context.Add(exam);
            _context.SaveChanges();
            return Ok("Insert Success");
        }

        [HttpGet("GetCorrectAnswer")]
        [Authorize]
        public IActionResult GetCorrectAnswer(int examId)
        {
            int start = (examId - 1) * 10;
            var correctAnswers = _context.Questions.Skip(start).Take(10).Select(q => q.CorrectAnswer).ToList();
            return Ok(correctAnswers);
        }

        [HttpGet("GetLastestExam")]
        public IActionResult GetLastestExam(int userId)
        {
            var exam = _context.Exams.Where(x => x.UserId.Equals(userId.ToString())).OrderByDescending(x => x.EndTime).FirstOrDefault();
            return Ok(exam);
        }

        [HttpGet("GetAllExamByUser")]
        [Authorize]
        public IActionResult GetAllExamByUser(int userId)
        {
            var exams = _context.Exams.Where(x => x.UserId.Equals(userId.ToString())).OrderByDescending(x => x.EndTime).ToList();
            return Ok(exams);
        }

        [HttpGet("GetExamByExamId")]
        public IActionResult GetExamByExamId(int examId)
        {
            var exam = _context.Exams.Where(x => x.ExamId == examId).FirstOrDefault();
            return Ok(exam);
        }


    }
}
