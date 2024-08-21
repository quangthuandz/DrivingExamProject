using System;
using System.Collections.Generic;

namespace ExamProject.Models
{
    public partial class Question
    {
        public Question()
        {
            ExamQuestions = new HashSet<ExamQuestion>();
        }

        public int QuestionId { get; set; }
        public string Content { get; set; } = null!;
        public int? CategoryId { get; set; }
        public string AnswerA { get; set; } = null!;
        public string AnswerB { get; set; } = null!;
        public string AnswerC { get; set; } = null!;
        public string AnswerD { get; set; } = null!;
        public string CorrectAnswer { get; set; } = null!;

        public virtual Category? Category { get; set; }
        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; }
    }
}
