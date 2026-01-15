namespace TriviaApi.Models;

public class AnswerRequest
{
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<int, string> Answers { get; set; } = new();
}
