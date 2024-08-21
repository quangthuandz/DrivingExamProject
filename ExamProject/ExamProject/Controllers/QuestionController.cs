using ExamProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExamProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ODataController
    {
        private DrivingExamQuizContext _context;
        public QuestionController(DrivingExamQuizContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet("Get")]
        public IActionResult Get(int examNumber)
        {
            int start = (examNumber - 1) * 10;
            var queryable = _context.Questions
                                    .Skip(start)
                                    .Take(10);
            return Ok(queryable);
        }

        [EnableQuery]
        [HttpGet("GetRandomExam")]
        public IActionResult GetRandomExam()
        {
            var queryable = _context.Questions
                                    .OrderBy(q => Guid.NewGuid())
                                    .Take(10);
            return Ok(queryable);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("GetAllQuestion")]
        [EnableQuery(PageSize = 10)]
        public IActionResult GetAllQuestion()
        {
            var queryable = _context.Questions.AsQueryable();
            return Ok(queryable);
        }

        [HttpPost]
        public IActionResult Post(Question question)
        {
            _context.Add(question);
            _context.SaveChanges();
            return Ok("Insert Success");
        }

        [HttpPut]
        public IActionResult Put(Question question)
        {
            var q = _context.Questions.Where(x => x.QuestionId == question.QuestionId).FirstOrDefault();
            if (q == null)
            {
                return BadRequest("Author not found");
            }
            q.Content = question.Content;
            q.AnswerA = question.AnswerA;
            q.AnswerB = question.AnswerB;
            q.AnswerC = question.AnswerC;
            q.AnswerD = question.AnswerD;
            q.CorrectAnswer = question.CorrectAnswer;
            q.CategoryId = null;
            _context.SaveChanges();
            return Ok("Update Success");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var a = _context.Questions.Where(x => x.QuestionId == id).FirstOrDefault();
            if (a == null)
            {
                return NotFound("Author not found");
            }
            _context.Questions.Remove(a);
            _context.SaveChanges();
            return Ok("Delete Success");
        }

        [HttpGet("{id}")]
        public IActionResult GetQuestionById(int id)
        {
            var q = _context.Questions.Where(x => x.QuestionId == id).FirstOrDefault();
            if(q == null)
            {
                return BadRequest();
            }
            return Ok(q);
        }


    }
}
