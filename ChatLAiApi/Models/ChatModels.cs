namespace ChatAIApi.Models
{
    public class QuestionRequest
    {
        public string Question { get; set; } = string.Empty;
        public List<ChatMessage>? History { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class OpenAiResponse
    {
        public Choice[]? Choices { get; set; }
    }

    public class Choice
    {
        public Message? Message { get; set; }
    }

    public class Message
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }
}
