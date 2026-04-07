using ChatAIApi.Controllers;
using ChatAIApi.Models;
using ChatAIApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace ChatAIApi.Tests.Controllers
{
    public class ChatAiControllerTests
    {
        private readonly Mock<IOpenRouterService> _serviceMock;
        private readonly ChatAiController _controller;

        public ChatAiControllerTests()
        {
            _serviceMock = new Mock<IOpenRouterService>();
            _controller = new ChatAiController(_serviceMock.Object);
        }

        [Fact]
        public async Task Post_EmptyQuestion_ReturnsBadRequest()
        {
            var request = new QuestionRequest { Question = "  " };

            var result = await _controller.Post(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [Fact]
        public async Task Post_ValidQuestion_ReturnsOkWithAnswer()
        {
            var request = new QuestionRequest { Question = "What is 2+2?" };
            _serviceMock
                .Setup(s => s.GetAnswerAsync(request.Question, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync("4");

            var result = await _controller.Post(request, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        }

        [Fact]
        public async Task Post_ServiceReturnsNull_Returns502()
        {
            var request = new QuestionRequest { Question = "Hello?" };
            _serviceMock
                .Setup(s => s.GetAnswerAsync(request.Question, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var result = await _controller.Post(request, CancellationToken.None);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status502BadGateway, statusResult.StatusCode);
        }

        [Fact]
        public async Task Post_ServiceThrowsHttpRequestException_ReturnsCorrespondingStatusCode()
        {
            var request = new QuestionRequest { Question = "Hello?" };
            _serviceMock
                .Setup(s => s.GetAnswerAsync(request.Question, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));

            var result = await _controller.Post(request, CancellationToken.None);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);
        }

        [Fact]
        public async Task Post_WithHistory_PassesHistoryToService()
        {
            var history = new List<ChatMessage>
            {
                new() { Role = "user", Content = "Hi" },
                new() { Role = "assistant", Content = "Hello!" }
            };
            var request = new QuestionRequest { Question = "How are you?", History = history };

            _serviceMock
                .Setup(s => s.GetAnswerAsync(request.Question, history, It.IsAny<CancellationToken>()))
                .ReturnsAsync("I'm doing great!");

            var result = await _controller.Post(request, CancellationToken.None);

            _serviceMock.Verify(s => s.GetAnswerAsync(request.Question, history, It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
