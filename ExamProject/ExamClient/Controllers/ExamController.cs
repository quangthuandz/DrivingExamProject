using ExamProject.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExamClient.Controllers
{
    public class ExamController : Controller
    {
        private static string[] questionIdListGlobal;

        private static int? flag;
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(string answers, string questionAnswers, string startTime, int examId, string questionIds, int? isRandomChecked)
        {
            var answerList = JsonConvert.DeserializeObject<Dictionary<int, string>>(answers);
            var questionAnswerList = JsonConvert.DeserializeObject<string[]>(questionAnswers);
            var questionIdList = JsonConvert.DeserializeObject<string[]>(questionIds); // Deserialize questionIds
            
            var randomId = questionIdList.ToArray();
            questionIdListGlobal = randomId;
            flag = isRandomChecked;
            var correctRandomAnswer = questionAnswerList.ToArray();
            var indices = answerList.Keys.ToArray();
            var answersArray = answerList.Values.ToArray();
            var examStartTime = DateTime.Parse(startTime);
            var userId = HttpContext.Session.GetString("UserId");
            var correctAnswers = await GetCorrectAnswer(examId);
            int score;
            if (isRandomChecked == 1)
            {
                score = CalculateRandomScore(questionAnswerList, correctAnswers);
            }
            else
            {
                score = CalculateScore(answerList, correctAnswers);
            }


            Exam e = new Exam()
            {
                StartTime = examStartTime,
                EndTime = DateTime.Now,
                Score = score,
                CreatedAt = null,
                UserId = userId,
                User = null,
            };

            var msg = await CreateExam(e);

            List<ExamQuestion> examQuestions = new List<ExamQuestion>();
            if(isRandomChecked == 1)
            {
                examQuestions = getAllRandomExamQuestion(examId, randomId, answersArray, correctRandomAnswer);
                
            }
            else
            {
                examQuestions = getAllExamQuestion(examId, indices, answersArray, correctAnswers);
            }
            

            

            foreach (var examQuestion in examQuestions)
            {
                var addEq = await CreateExamQuestion(examQuestion);
            }

            HttpContext.Session.SetInt32("examId", examId);

            return RedirectToAction("Result", "Exam");
        }

        internal async Task<string> CreateExam(Exam e)
        {
            var link = "https://localhost:5000/api/Exam";
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.PostAsJsonAsync(link, e))
                {
                    return "Insert Success";
                }
            }
        }

        internal async Task<string> CreateExamQuestion(ExamQuestion eq)
        {
            var link = "https://localhost:5000/api/ExamQuestion";
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.PostAsJsonAsync(link, eq))
                {
                    return "Insert Success";
                }
            }
        }

        private List<ExamQuestion> getAllRandomExamQuestion(int examId, string[] randomId, string[] userAnswers, string[] correctAnswers)
        {
            var examQuestions = new List<ExamQuestion>();
            var userId = HttpContext.Session.GetString("UserId");
            for (int i = 0; i < randomId.Length; i++)
            {
                var examQuestion = new ExamQuestion
                {
                    ExamId = GetLastestExam(int.Parse(userId)).Result.ExamId,
                    QuestionId = int.Parse(randomId[i]),
                    UserAnswer = userAnswers[i],
                    Correct = userAnswers[i] == correctAnswers[i]
                };

                examQuestions.Add(examQuestion);
            }

            return examQuestions;
        }
        private List<ExamQuestion> getAllExamQuestion(int examId, int[] indices, string[] userAnswers, List<string> correctAnswers)
        {
            var examQuestions = new List<ExamQuestion>();
            var userId = HttpContext.Session.GetString("UserId");
            for (int i = 0; i < indices.Length; i++)
            {
                var examQuestion = new ExamQuestion
                {
                    ExamId = GetLastestExam(int.Parse(userId)).Result.ExamId,
                    QuestionId = indices[i]+10*examId -9,
                    UserAnswer = userAnswers[i],
                    Correct = userAnswers[i] == correctAnswers[i]
                };

                examQuestions.Add(examQuestion);
            }

            return examQuestions;
        }


        private int CalculateRandomScore(string[] questionAnswers, List<string> correctAnswers)
        {
            int score = 0;

            for (int i = 0; i < correctAnswers.Count && i < questionAnswers.Length; i++)
            {
                if (questionAnswers[i] == correctAnswers[i])
                {
                    score++;
                }
            }

            return score;
        }

        private int CalculateScore(Dictionary<int, string> answerList, List<string> correctAnswers)
        {
            int score = 0;

            var answersArray = answerList.Values.ToArray();

            for (int i = 0; i < correctAnswers.Count && i < answersArray.Length; i++)
            {
                if (answersArray[i] == correctAnswers[i])
                {
                    score++;
                }
            }

            return score;
        }

        internal async Task<List<string>> GetCorrectAnswer(int id)
         {
            var link = "https://localhost:5000/api/Exam/GetCorrectAnswer?examId=" + id;
            using (HttpClient client = new HttpClient())
            {
                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("Token is not available.");
                }

                // Thêm token vào header của yêu cầu HTTP
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<string> correct = JsonConvert.DeserializeObject<List<string>>(data);
                        return correct;
                    }
                }
            }
        }

        public IActionResult Result()
        {
            var userId = HttpContext.Session.GetString("UserId");
            Exam exam = GetLastestExam(int.Parse(userId)).Result;

            if (exam == null)
            {
                return NotFound("Không tìm thấy thông tin bài kiểm tra.");
            }

            TimeSpan? time = exam.EndTime - exam.StartTime;
            ViewBag.Score = exam.Score;
            ViewBag.Time = time;

            return View();
        }



        internal async Task<Exam> GetLastestExam(int userId)
        {
            var link = "https://localhost:5000/api/Exam/GetLastestExam?userId=" +userId;
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        Exam e = JsonConvert.DeserializeObject<Exam>(data);
                        return e;
                    }
                }
            }
        }

        internal async Task<List<Question>> GetRandomQuestion()
        {
            var link = "https://localhost:5000/api/Question/GetRandomExam";
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<Question> e = JsonConvert.DeserializeObject<List<Question>>(data);
                        return e;
                    }
                }
            }
        }

        internal async Task<List<ExamQuestion>> GetResultQuestionByExamId(int examId)
        {
            var link = "https://localhost:5000/api/ExamQuestion/GetQuestionResultByExamId?examId=" +examId;
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<ExamQuestion> eq = JsonConvert.DeserializeObject<List<ExamQuestion>>(data);
                        return eq;
                    }
                }
            }
        }

        internal async Task<List<Question>> GetHistoryQuestion(int? examId)
        {
            var link = "https://localhost:5000/api/Question/Get?examNumber=" +examId;
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<Question> q = JsonConvert.DeserializeObject<List<Question>>(data);
                        return q;
                    }
                }
            }
        }

        public async Task<IActionResult> History(int examId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int? sessionExamId = HttpContext.Session.GetInt32("examId");
            List<Question> randomQuestion = new List<Question>();
            for(int i = 0; i < questionIdListGlobal.Length; i++)
            {
                Question q = GetQuestionById(int.Parse(questionIdListGlobal[i])).Result;
                randomQuestion.Add(q);
            }
            List<Question> questions = new List<Question>();
            List<ExamQuestion> examQuestions = new List<ExamQuestion>();
            Exam exam = new Exam();
            if (flag == 1)
            {
                questions = randomQuestion;
                examQuestions = await GetResultQuestionByExamId(GetLastestExam(int.Parse(userId)).Result.ExamId);
                exam = await GetExamByExamId(GetLastestExam(int.Parse(userId)).Result.ExamId);
            }
            else if(examId != 0)
            {
                List<ExamQuestion> eq = await GetResultQuestionByExamId(examId);
                int? urlExamId = 0;
                urlExamId = (eq[0].QuestionId - 1) / 10 + 1;
                questions = await GetHistoryQuestion(urlExamId);
                examQuestions = await GetResultQuestionByExamId(examId);
                exam = await GetExamByExamId(examId);
            }
            else if(examId == 0)
            {
                questions = await GetHistoryQuestion(sessionExamId);
                examQuestions = await GetResultQuestionByExamId(GetLastestExam(int.Parse(userId)).Result.ExamId);
                exam = await GetExamByExamId(GetLastestExam(int.Parse(userId)).Result.ExamId);
            }

            ViewBag.Score = exam.Score;
            ViewBag.StartDate = exam.StartTime.ToString("dd/MM/yyyy");

            var questionViewModels = questions.Select(q => new 
            {
                QuestionId = q.QuestionId,
                Content = q.Content,
                AnswerA = q.AnswerA,
                AnswerB = q.AnswerB,
                AnswerC = q.AnswerC,
                AnswerD = q.AnswerD,
                CorrectAnswer = q.CorrectAnswer,
                SelectedAnswer = examQuestions.FirstOrDefault(eq => eq.QuestionId == q.QuestionId)?.UserAnswer,
                IsCorrect = examQuestions.FirstOrDefault(eq => eq.QuestionId == q.QuestionId)?.UserAnswer == q.CorrectAnswer
            }).ToList();

            return View(questionViewModels);
        }

        public async Task<IActionResult> ListHistoryExam()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int? examId = HttpContext.Session.GetInt32("examId");

            List<Exam> exams = await GetAllExamByUser(int.Parse(userId));

            if (exams == null)
            {
                return StatusCode(401, "Unauthorized");
            }


            return View(exams);
        }

        internal async Task<List<Exam>> GetAllExamByUser(int? userId)
        {
            var link = "https://localhost:5000/api/Exam/GetAllExamByUser?userId=" + userId;
            using (HttpClient client = new HttpClient())
            {
                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                // Thêm token vào header của yêu cầu HTTP
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    if (res.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return null;
                    }
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<Exam> e = JsonConvert.DeserializeObject<List<Exam>>(data);
                        return e;
                    }
                }
            }
        }
        internal async Task<Exam> GetExamByExamId(int? examId)
        {
            var link = "https://localhost:5000/api/Exam/GetExamByExamId?examId=" + examId;
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        Exam e = JsonConvert.DeserializeObject<Exam>(data);
                        return e;
                    }
                }
            }
        }

        internal async Task<Question> GetQuestionById(int? questionId)
        {
            var link = "https://localhost:5000/api/Question/" + questionId;
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(link))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        Question e = JsonConvert.DeserializeObject<Question>(data);
                        return e;
                    }
                }
            }
        }


    }
}
