using ChatAIApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatAIApi.Services
{
    public class OpenRouterService : IOpenRouterService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenRouterService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public OpenRouterService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OpenRouterService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> GetAnswerAsync(string question, List<ChatMessage>? history, CancellationToken cancellationToken)
        {
            var model = _configuration["OpenRouter:Model"] ?? "nvidia/nemotron-3-super-120b-a12b:free";
            var systemPrompt = _configuration["OpenRouter:SystemPrompt"];

            var messages = new List<object>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
                messages.Add(new { role = "system", content = systemPrompt });

            if (history?.Count > 0)
                foreach (var msg in history)
                    messages.Add(new { role = msg.Role, content = msg.Content });

            messages.Add(new { role = "user", content = question });

            var body = new { model, messages };

            var client = _httpClientFactory.CreateClient("OpenRouter");
            var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json")
            };

            _logger.LogInformation("Sending request to OpenRouter. Model: {Model}", model);

            var response = await client.SendAsync(request, cancellationToken);
            var resultString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenRouter returned {StatusCode}: {Body}", (int)response.StatusCode, resultString);
                throw new HttpRequestException(resultString, null, response.StatusCode);
            }

            var parsed = JsonSerializer.Deserialize<OpenAiResponse>(resultString, _jsonOptions);
            return parsed?.Choices?[0]?.Message?.Content;
        }
    }
}
