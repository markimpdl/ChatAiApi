using ChatLlamaApi.Models;
using ChatLlamaApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ChatLlamaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatAiController : ControllerBase
    {
        private readonly IOpenRouterService _openRouterService;

        public ChatAiController(IOpenRouterService openRouterService)
        {
            _openRouterService = openRouterService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Post([FromBody] QuestionRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { error = "Question cannot be empty." });

            try
            {
                var answer = await _openRouterService.GetAnswerAsync(request.Question, request.History, cancellationToken);

                if (answer is null)
                    return StatusCode(StatusCodes.Status502BadGateway, new { error = "No response from model." });

                return Ok(new { answer });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? HttpStatusCode.BadGateway), new { error = ex.Message });
            }
        }
    }
}
