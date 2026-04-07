using ChatLlamaApi.Models;

namespace ChatLlamaApi.Services
{
    public interface IOpenRouterService
    {
        Task<string?> GetAnswerAsync(string question, List<ChatMessage>? history, CancellationToken cancellationToken);
    }
}
