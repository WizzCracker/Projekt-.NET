// Controllers/GeminiChatController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization; // Added for JsonPropertyName

namespace YourProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GeminiChatController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var apiKey = Environment.GetEnvironmentVariable("GoogleApiKeys__GeminiApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, "Gemini API key is not configured.");
            }

            var geminiProModel = "gemini-2.5-flash-preview-05-20";
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiProModel}:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = request.Prompt + "\n" + request.UserMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 200
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode(); // This will still throw for non-2xx HTTP codes

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // --- NEW LOGIC START ---
                if (geminiResponse?.PromptFeedback?.SafetyRatings?.Any() == true)
                {
                    // Log the safety ratings for debugging if needed
                    foreach (var rating in geminiResponse.PromptFeedback.SafetyRatings)
                    {
                        if (rating.Blocked == true) // Check if this specific category caused a block
                        {
                            Console.WriteLine($"Gemini blocked content due to: {rating.Category} (Probability: {rating.Probability})");
                        }
                    }
                    return StatusCode(400, "I'm sorry, I cannot provide a response to that request due to content policy. Please try rephrasing your message.");
                }
                // --- NEW LOGIC END ---


                var generatedText = geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text;

                if (!string.IsNullOrEmpty(generatedText))
                {
                    return Ok(new { geminiResponse = generatedText });
                }
                else
                {
                    return StatusCode(500, "Gemini did not return valid content. This might be due to an unexpected API response format or internal model issue.");
                }
            }
            catch (HttpRequestException e)
            {
                // This catches network errors or non-2xx HTTP responses from Google's API (e.g., 403, 404, 500 from Google)
                return StatusCode(500, $"Error communicating with Gemini API: {e.Message}");
            }
            catch (JsonException e)
            {
                // This catches errors in parsing Google's JSON response
                return StatusCode(500, $"Error parsing Gemini API response: {e.Message}");
            }
            catch (System.Exception e)
            {
                // Catch any other unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {e.Message}");
            }
        }
    }

    // DTOs for request and response
    public class ChatRequest
    {
        public string UserMessage { get; set; }
        public string Prompt { get; set; }
    }

    // --- UPDATED DTOs for Gemini Response ---
    public class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
        public PromptFeedback? PromptFeedback { get; set; } // Added for safety feedback
    }

    public class Candidate
    {
        public Content? Content { get; set; }
        public string? FinishReason { get; set; } // Added, good for debugging (e.g., "SAFETY")
        public SafetyRating[]? SafetyRatings { get; set; } // Safety ratings for *generated content*
    }

    public class Content
    {
        public Part[]? Parts { get; set; }
        public string? Role { get; set; }
    }

    public class Part
    {
        public string? Text { get; set; }
    }

    public class PromptFeedback // New DTO for prompt-level feedback
    {
        public SafetyRating[]? SafetyRatings { get; set; }
    }

    public class SafetyRating // New DTO for safety details
    {
        // Use JsonPropertyName to map PascalCase C# properties to snake_case JSON
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("probability")]
        public string? Probability { get; set; }

        [JsonPropertyName("blocked")] // Indicates if this specific category caused the content to be blocked
        public bool? Blocked { get; set; }
    }
}
