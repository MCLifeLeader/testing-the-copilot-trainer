namespace MyChatApp.Web.Models;

public record ChatMessage(string Content, bool IsUser, DateTime Timestamp);

public record SendMessageRequest(string Content);

public record ChatResponse(string Content, DateTime Timestamp);