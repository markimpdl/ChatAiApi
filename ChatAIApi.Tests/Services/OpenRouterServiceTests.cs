using ChatAIApi.Models;
using ChatAIApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ChatAIApi.Tests.Services
{
    public class OpenRouterServiceTests
    {
        private static OpenRouterService CreateService(HttpMessageHandler handler, Dictionary<string, string?>? config = null)
        {
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://openrouter.ai/api/v1/")
            };

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("OpenRouter")).Returns(httpClient);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config ?? new Dictionary<string, string?>
                {
                    ["OpenRouter:Model"] = "test-model:free",
                    ["OpenRouter:SystemPrompt"] = null
                })
                .Build();

            return new OpenRouterService(factoryMock.Object, configuration, NullLogger<OpenRouterService>.Instance);
        }

        private static HttpMessageHandler CreateHandler(HttpStatusCode statusCode, object body)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                });
            return handlerMock.Object;
        }

        [Fact]
        public async Task GetAnswerAsync_ValidResponse_ReturnsContent()
        {
            var responseBody = new
            {
                choices = new[]
                {
                    new { message = new { role = "assistant", content = "Hello there!" } }
                }
            };
            var service = CreateService(CreateHandler(HttpStatusCode.OK, responseBody));

            var result = await service.GetAnswerAsync("Hi", null, CancellationToken.None);

            Assert.Equal("Hello there!", result);
        }

        [Fact]
        public async Task GetAnswerAsync_ApiReturnsError_ThrowsHttpRequestException()
        {
            var service = CreateService(CreateHandler(HttpStatusCode.Unauthorized, new { error = "Invalid key" }));

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetAnswerAsync("Hi", null, CancellationToken.None));
        }

        [Fact]
        public async Task GetAnswerAsync_WithSystemPrompt_IncludesSystemMessage()
        {
            HttpRequestMessage? capturedRequest = null;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        choices = new[] { new { message = new { role = "assistant", content = "ok" } } }
                    }), Encoding.UTF8, "application/json")
                });

            var service = CreateService(handlerMock.Object, new Dictionary<string, string?>
            {
                ["OpenRouter:Model"] = "test-model:free",
                ["OpenRouter:SystemPrompt"] = "You are a test assistant."
            });

            await service.GetAnswerAsync("Hello", null, CancellationToken.None);

            Assert.NotNull(capturedRequest);
            var bodyString = await capturedRequest!.Content!.ReadAsStringAsync();
            Assert.Contains("system", bodyString);
            Assert.Contains("You are a test assistant.", bodyString);
        }

        [Fact]
        public async Task GetAnswerAsync_WithHistory_IncludesHistoryInMessages()
        {
            HttpRequestMessage? capturedRequest = null;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        choices = new[] { new { message = new { role = "assistant", content = "ok" } } }
                    }), Encoding.UTF8, "application/json")
                });

            var service = CreateService(handlerMock.Object);
            var history = new List<ChatMessage>
            {
                new() { Role = "user", Content = "Previous question" },
                new() { Role = "assistant", Content = "Previous answer" }
            };

            await service.GetAnswerAsync("New question", history, CancellationToken.None);

            var bodyString = await capturedRequest!.Content!.ReadAsStringAsync();
            Assert.Contains("Previous question", bodyString);
            Assert.Contains("Previous answer", bodyString);
            Assert.Contains("New question", bodyString);
        }
    }
}
