using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InventoryManagementSystem.Services.ZenoAIService
{
    public sealed class ZenoAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ZenoAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string?> AskAsync(string userMessage, string businessContext, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["OpenAI:ApiKey"]
                         ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            var model = _configuration["OpenAI:Model"] ?? "gpt-5.6-luna";

            var systemPrompt = "You are ZenoAI, the intelligent assistant for the Invenzo inventory management system. " +
                               "Answer using only the supplied live business context. Never invent database values. " +
                               "Be concise, professional and helpful. Use INR (₹) for money. " +
                               "You can explain inventory, sales, revenue, purchases, invoices, payments and low-stock status. " +
                               "If the user asks to generate a report, tell the application to use its report download feature rather than pretending a file was created.";

            var input = $"SYSTEM INSTRUCTIONS:\n{systemPrompt}\n\nLIVE DATABASE CONTEXT:\n{businessContext}\n\nUSER QUESTION:\n{userMessage}";

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { model, input }),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return FindText(document.RootElement);
        }

        private static string? FindText(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals("text") && property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                            return value;
                    }

                    var nested = FindText(property.Value);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var nested = FindText(item);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }

            return null;
        }
    }
}
