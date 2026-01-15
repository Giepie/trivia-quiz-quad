using System.Text.Json.Serialization;

namespace TriviaApi.Models;

// Models for deserializing Open Trivia Database API response
public class OpenTriviaResponse
{
    [JsonPropertyName("response_code")]
    public int ResponseCode { get; set; }
    
    [JsonPropertyName("results")]
    public List<OpenTriviaQuestion> Results { get; set; } = new();
}

public class OpenTriviaQuestion
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
    
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;
    
    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; } = string.Empty;
    
    [JsonPropertyName("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; } = new();
}
