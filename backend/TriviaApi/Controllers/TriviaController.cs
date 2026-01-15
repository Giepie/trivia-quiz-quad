using Microsoft.AspNetCore.Mvc;
using TriviaApi.Models;
using TriviaApi.Services;

namespace TriviaApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TriviaController : ControllerBase
{
    private readonly TriviaService _triviaService;
    private readonly ILogger<TriviaController> _logger;

    public TriviaController(TriviaService triviaService, ILogger<TriviaController> logger)
    {
        _triviaService = triviaService;
        _logger = logger;
    }

    /// <summary>
    /// Get trivia questions. Returns a session ID and questions without revealing correct answers.
    /// </summary>
    /// <param name="amount">Number of questions to retrieve (1-50, default: 10)</param>
    /// <param name="difficulty">Optional difficulty level: easy, medium, or hard</param>
    /// <param name="type">Optional question type: multiple or boolean</param>
    /// <returns>Quiz response with session ID and list of questions with shuffled answers</returns>
    [HttpGet("questions")]
    [ProducesResponseType(typeof(QuizResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuizResponse>> GetQuestions(
        [FromQuery] int amount = 10,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? type = null)
    {
        if (amount < 1 || amount > 50)
        {
            return BadRequest("Amount must be between 1 and 50");
        }

        if (!string.IsNullOrEmpty(difficulty) && 
            difficulty != "easy" && difficulty != "medium" && difficulty != "hard")
        {
            return BadRequest("Difficulty must be 'easy', 'medium', or 'hard'");
        }

        if (!string.IsNullOrEmpty(type) && 
            type != "multiple" && type != "boolean")
        {
            return BadRequest("Type must be 'multiple' or 'boolean'");
        }

        var response = await _triviaService.GetQuestionsAsync(amount, difficulty, type);
        
        if (response == null)
        {
            return StatusCode(500, "Failed to fetch trivia questions");
        }

        return Ok(response);
    }

    /// <summary>
    /// Check answers for a quiz session and get the score.
    /// </summary>
    /// <param name="request">Answer request containing session ID and user's answers mapped by question ID</param>
    /// <returns>Score, total questions, and detailed results showing correct/incorrect answers</returns>
    [HttpPost("checkanswers")]
    [ProducesResponseType(typeof(AnswerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AnswerResponse> CheckAnswers([FromBody] AnswerRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
        {
            return BadRequest("Session ID is required");
        }

        var response = _triviaService.CheckAnswers(request);
        
        if (response == null)
        {
            return NotFound("Session not found or expired");
        }

        return Ok(response);
    }
}
