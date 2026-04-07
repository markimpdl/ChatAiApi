using ChatAIApi.Models;

namespace ChatAIApi.Services
{
    public interface IOpenRouterService
    {
        Task<string?> GetAnswerAsync(string question, List<ChatMessage>? history, CancellationToken cancellationToken);
    }
}
