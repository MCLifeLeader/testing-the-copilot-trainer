using MyChatApp.Web.Models;

namespace MyChatApp.Web.Repository;

public class ChatApiClient(HttpClient httpClient)
{
    public async Task<ChatResponse?> SendMessageAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            var request = new SendMessageRequest(content);
            var response = await httpClient.PostAsJsonAsync("/api/chat/send", request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken);
            }

            return null;
        }
        catch
        {
            // Return null if API call fails
            return null;
        }
    }

    public async Task<string[]> GetRandomResponsesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var responses = await httpClient.GetFromJsonAsync<string[]>("/api/chat/responses", cancellationToken);
            return responses ?? [];
        }
        catch
        {
            // Return fallback responses if API call fails
            return [
                "That's interesting! Can you tell me more?",
                "I understand. Is there anything specific you'd like to know?",
                "Thanks for sharing that with me.",
                "That's a great question! Let me think about that.",
                "I see what you mean. How can I help you with that?"
            ];
        }
    }
}