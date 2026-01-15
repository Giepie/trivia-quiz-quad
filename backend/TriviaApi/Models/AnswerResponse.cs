namespace TriviaApi.Models;

public class AnswerResponse
{
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuestionResult> Results { get; set; } = new();
}

public class QuestionResult
{
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? UserAnswer { get; set; }
}
