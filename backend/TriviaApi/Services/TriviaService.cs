using System.Net.Http.Json;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using TriviaApi.Models;

namespace TriviaApi.Services;

public class TriviaService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TriviaService> _logger;
    private const string OPEN_TRIVIA_API_URL = "https://opentdb.com/api.php";
    private const int CACHE_EXPIRATION_MINUTES = 30;

    public TriviaService(HttpClient httpClient, IMemoryCache cache, ILogger<TriviaService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<QuizResponse?> GetQuestionsAsync(int amount = 10, string? difficulty = null, string? type = null)
    {
        try
        {
            var url = $"{OPEN_TRIVIA_API_URL}?amount={amount}";
            
            if (!string.IsNullOrEmpty(difficulty))
            {
                url += $"&difficulty={difficulty}";
            }
            
            if (!string.IsNullOrEmpty(type))
            {
                url += $"&type={type}";
            }

            _logger.LogInformation("Fetching questions from Open Trivia API: {url}", url);

            var response = await _httpClient.GetFromJsonAsync<OpenTriviaResponse>(url);

            if (response == null || response.ResponseCode != 0)
            {
                _logger.LogError("Failed to fetch questions from Open Trivia API. Response code: {code}", 
                    response?.ResponseCode ?? -1);
                return null;
            }

            var sessionId = Guid.NewGuid().ToString();

            var session = new QuizSession
            {
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                Questions = new List<QuestionDto>(),
                CorrectAnswers = new Dictionary<int, string>()
            };

            for (int i = 0; i < response.Results.Count; i++)
            {
                var triviaQuestion = response.Results[i];
                
                // Decode HTML entities in question and answers
                var decodedQuestion = HttpUtility.HtmlDecode(triviaQuestion.Question);
                var decodedCorrectAnswer = HttpUtility.HtmlDecode(triviaQuestion.CorrectAnswer);
                var decodedIncorrectAnswers = triviaQuestion.IncorrectAnswers
                    .Select(a => HttpUtility.HtmlDecode(a))
                    .ToList();

                // Shuffle answers to hide the correct one
                var allAnswers = new List<string> { decodedCorrectAnswer };
                allAnswers.AddRange(decodedIncorrectAnswers);
                allAnswers = allAnswers.OrderBy(_ => Random.Shared.Next()).ToList();

                session.CorrectAnswers[i] = decodedCorrectAnswer;

                var questionDto = new QuestionDto
                {
                    Id = i,
                    Question = decodedQuestion,
                    Answers = allAnswers,
                    Category = HttpUtility.HtmlDecode(triviaQuestion.Category),
                    Difficulty = triviaQuestion.Difficulty
                };

                session.Questions.Add(questionDto);
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
            
            _cache.Set(sessionId, session, cacheOptions);

            _logger.LogInformation("Created quiz session {sessionId} with {count} questions", 
                sessionId, session.Questions.Count);

            return new QuizResponse
            {
                SessionId = sessionId,
                Questions = session.Questions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching trivia questions");
            return null;
        }
    }

    public AnswerResponse? CheckAnswers(AnswerRequest request)
    {
        try
        {
            if (!_cache.TryGetValue<QuizSession>(request.SessionId, out var session) || session == null)
            {
                _logger.LogWarning("Session {sessionId} not found or expired", request.SessionId);
                return null;
            }

            var results = new List<QuestionResult>();
            int correctCount = 0;

            foreach (var answer in request.Answers)
            {
                var questionId = answer.Key;
                var userAnswer = answer.Value;
                
                if (session.CorrectAnswers.TryGetValue(questionId, out var correctAnswer))
                {
                    var isCorrect = string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
                    
                    if (isCorrect)
                    {
                        correctCount++;
                    }

                    results.Add(new QuestionResult
                    {
                        QuestionId = questionId,
                        IsCorrect = isCorrect,
                        CorrectAnswer = correctAnswer,
                        UserAnswer = userAnswer
                    });
                }
            }

            _logger.LogInformation("Session {sessionId}: User scored {score}/{total}", 
                request.SessionId, correctCount, session.CorrectAnswers.Count);

            return new AnswerResponse
            {
                Score = correctCount,
                TotalQuestions = session.CorrectAnswers.Count,
                Results = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking answers for session {sessionId}", request.SessionId);
            return null;
        }
    }
}
