using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TriviaApi.Models;
using TriviaApi.Services;
using Xunit;

namespace TriviaApi.Tests;

public class TriviaServiceTests
{
    private readonly Mock<ILogger<TriviaService>> _mockLogger;
    private readonly IMemoryCache _cache;

    public TriviaServiceTests()
    {
        _mockLogger = new Mock<ILogger<TriviaService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task GetQuestionsAsync_ShouldReturnQuestions_WhenApiReturnsValidData()
    {
        // Arrange
        var mockResponse = new OpenTriviaResponse
        {
            ResponseCode = 0,
            Results = new List<OpenTriviaQuestion>
            {
                new OpenTriviaQuestion
                {
                    Category = "Science",
                    Difficulty = "easy",
                    Question = "What is the chemical symbol for gold?",
                    CorrectAnswer = "Au",
                    IncorrectAnswers = new List<string> { "Ag", "Fe", "Cu" }
                },
                new OpenTriviaQuestion
                {
                    Category = "History",
                    Difficulty = "medium",
                    Question = "Who was the first president?",
                    CorrectAnswer = "George Washington",
                    IncorrectAnswers = new List<string> { "John Adams", "Thomas Jefferson", "James Madison" }
                }
            }
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(mockResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new TriviaService(httpClient, _cache, _mockLogger.Object);

        // Act
        var result = await service.GetQuestionsAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.SessionId);
        Assert.Equal(2, result.Questions.Count);
        
        // Verify first question
        var firstQuestion = result.Questions[0];
        Assert.Equal(0, firstQuestion.Id);
        Assert.Equal("What is the chemical symbol for gold?", firstQuestion.Question);
        Assert.Equal(4, firstQuestion.Answers.Count); // 1 correct + 3 incorrect
        Assert.Contains("Au", firstQuestion.Answers);
        
        // Verify answers are shuffled (all answers should be present)
        Assert.Contains("Au", firstQuestion.Answers);
        Assert.Contains("Ag", firstQuestion.Answers);
        Assert.Contains("Fe", firstQuestion.Answers);
        Assert.Contains("Cu", firstQuestion.Answers);
    }

    [Fact]
    public async Task GetQuestionsAsync_ShouldReturnNull_WhenApiReturnsErrorCode()
    {
        // Arrange
        var mockResponse = new OpenTriviaResponse
        {
            ResponseCode = 1, // Error code
            Results = new List<OpenTriviaQuestion>()
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(mockResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new TriviaService(httpClient, _cache, _mockLogger.Object);

        // Act
        var result = await service.GetQuestionsAsync(10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CheckAnswers_ShouldReturnCorrectScore_WhenAnswersAreCorrect()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var session = new QuizSession
        {
            SessionId = sessionId,
            CorrectAnswers = new Dictionary<int, string>
            {
                { 0, "Au" },
                { 1, "George Washington" }
            },
            Questions = new List<QuestionDto>()
        };

        _cache.Set(sessionId, session);

        var httpClient = new HttpClient();
        var service = new TriviaService(httpClient, _cache, _mockLogger.Object);

        var answerRequest = new AnswerRequest
        {
            SessionId = sessionId,
            Answers = new Dictionary<int, string>
            {
                { 0, "Au" },
                { 1, "George Washington" }
            }
        };

        // Act
        var result = service.CheckAnswers(answerRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Score);
        Assert.Equal(2, result.TotalQuestions);
        Assert.All(result.Results, r => Assert.True(r.IsCorrect));
    }

    [Fact]
    public void CheckAnswers_ShouldReturnPartialScore_WhenSomeAnswersAreIncorrect()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var session = new QuizSession
        {
            SessionId = sessionId,
            CorrectAnswers = new Dictionary<int, string>
            {
                { 0, "Au" },
                { 1, "George Washington" }
            },
            Questions = new List<QuestionDto>()
        };

        _cache.Set(sessionId, session);

        var httpClient = new HttpClient();
        var service = new TriviaService(httpClient, _cache, _mockLogger.Object);

        var answerRequest = new AnswerRequest
        {
            SessionId = sessionId,
            Answers = new Dictionary<int, string>
            {
                { 0, "Au" },
                { 1, "John Adams" } // Incorrect
            }
        };

        // Act
        var result = service.CheckAnswers(answerRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Score);
        Assert.Equal(2, result.TotalQuestions);
        Assert.True(result.Results[0].IsCorrect);
        Assert.False(result.Results[1].IsCorrect);
    }

    [Fact]
    public void CheckAnswers_ShouldReturnNull_WhenSessionNotFound()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new TriviaService(httpClient, _cache, _mockLogger.Object);

        var answerRequest = new AnswerRequest
        {
            SessionId = "non-existent-session",
            Answers = new Dictionary<int, string>()
        };

        // Act
        var result = service.CheckAnswers(answerRequest);

        // Assert
        Assert.Null(result);
    }
}
