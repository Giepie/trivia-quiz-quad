namespace TriviaApi.Models;

public class QuizSession
{
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<int, string> CorrectAnswers { get; set; } = new();
    public List<QuestionDto> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
