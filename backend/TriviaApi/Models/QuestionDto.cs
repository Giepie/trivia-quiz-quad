namespace TriviaApi.Models;

public class QuestionDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Answers { get; set; } = new();
    public string? Category { get; set; }
    public string? Difficulty { get; set; }
}
