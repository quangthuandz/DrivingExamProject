using System;
using System.Collections.Generic;

namespace ExamProject.Models
{
    public partial class Exam
    {
        public Exam()
        {
            ExamQuestions = new HashSet<ExamQuestion>();
        }

        public int ExamId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Score { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string UserId { get; set; }
        public User? User { get; set; }

        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; }
    }
}
