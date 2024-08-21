using System;
using System.Collections.Generic;

namespace ExamProject.Models
{
    public partial class ExamQuestion
    {
        public int ExamQuestionId { get; set; }
        public int? ExamId { get; set; }
        public int? QuestionId { get; set; }
        public string? UserAnswer { get; set; }
        public bool? Correct { get; set; }

        public virtual Exam? Exam { get; set; }
        public virtual Question? Question { get; set; }
    }
}
