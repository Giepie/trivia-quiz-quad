namespace TriviaApi.Models;

public class QuizResponse
{
    public string SessionId { get; set; } = string.Empty;
    public List<QuestionDto> Questions { get; set; } = new();
}
