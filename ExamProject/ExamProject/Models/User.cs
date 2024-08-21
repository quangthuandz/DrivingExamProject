using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ExamProject.Models
{
    public partial class User : IdentityUser
    {
        public string Password { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!;

        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    }
}
