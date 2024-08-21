using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ExamProject.Models
{
    public partial class DrivingExamQuizContext : DbContext
    {
        public DrivingExamQuizContext()
        {
        }

        public DrivingExamQuizContext(DbContextOptions<DrivingExamQuizContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Exam> Exams { get; set; } = null!;
        public virtual DbSet<ExamQuestion> ExamQuestions { get; set; } = null!;
        public virtual DbSet<Question> Questions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(config.GetConnectionString("value"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");

                entity.HasIndex(e => e.Name, "UQ__category__72E12F1BCEF387DF")
                    .IsUnique();

                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Exam>(entity =>
            {
                entity.ToTable("exam");

                entity.Property(e => e.ExamId).HasColumnName("exam_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EndTime)
                    .HasColumnType("datetime")
                    .HasColumnName("end_time");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.Property(e => e.StartTime)
                    .HasColumnType("datetime")
                    .HasColumnName("start_time");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                      .WithMany(p => p.Exams)
                      .HasForeignKey(d => d.UserId)
                      .HasConstraintName("FK_exam_user");
            });

            modelBuilder.Entity<ExamQuestion>(entity =>
            {
                entity.ToTable("exam_question");

                entity.Property(e => e.ExamQuestionId).HasColumnName("exam_question_id");

                entity.Property(e => e.Correct).HasColumnName("correct");

                entity.Property(e => e.ExamId).HasColumnName("exam_id");

                entity.Property(e => e.QuestionId).HasColumnName("question_id");

                entity.Property(e => e.UserAnswer)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("user_answer")
                    .IsFixedLength();

                entity.HasOne(d => d.Exam)
                    .WithMany(p => p.ExamQuestions)
                    .HasForeignKey(d => d.ExamId)
                    .HasConstraintName("FK__exam_ques__exam___4316F928");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.ExamQuestions)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("FK__exam_ques__quest__440B1D61");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("question");

                entity.Property(e => e.QuestionId).HasColumnName("question_id");

                entity.Property(e => e.AnswerA)
                    .HasColumnType("text")
                    .HasColumnName("answer_a");

                entity.Property(e => e.AnswerB)
                    .HasColumnType("text")
                    .HasColumnName("answer_b");

                entity.Property(e => e.AnswerC)
                    .HasColumnType("text")
                    .HasColumnName("answer_c");

                entity.Property(e => e.AnswerD)
                    .HasColumnType("text")
                    .HasColumnName("answer_d");

                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.CorrectAnswer)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("correct_answer")
                    .IsFixedLength();

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__question__catego__3D5E1FD2");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id).HasColumnName("user_id");

                entity.HasIndex(e => e.EmailAddress, "UQ__user__20C6DFF5D5D63BC1")
                    .IsUnique();

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email_address");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("role");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
