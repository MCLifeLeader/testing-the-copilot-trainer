using Microsoft.AspNetCore.Mvc;
using MyChatApp.ApiService.Models;

namespace MyChatApp.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private static readonly string[] Responses = [
        "That's interesting! Can you tell me more?",
        "I understand. Is there anything specific you'd like to know?",
        "Thanks for sharing that with me.",
        "That's a great question! Let me think about that.",
        "I see what you mean. How can I help you with that?"
    ];

    [HttpPost("send")]
    public ActionResult<ChatResponse> SendMessage([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Message content cannot be empty.");
        }

        // Simulate processing time
        Thread.Sleep(500);

        // Return a random response
        var random = new Random();
        var responseContent = Responses[random.Next(Responses.Length)];
        
        return Ok(new ChatResponse(responseContent, DateTime.Now));
    }

    [HttpGet("responses")]
    public ActionResult<IEnumerable<string>> GetRandomResponses()
    {
        return Ok(Responses);
    }
}