using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PortlinkApp.Core.Services;

public class LMStudioAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LMStudioAIService> _logger;
    private const string LmStudioUrl = "http://127.0.0.1:1234/v1/chat/completions";

    public LMStudioAIService(HttpClient httpClient, ILogger<LMStudioAIService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger = logger;
    }

    public async Task<bool> IsAvailable()
    {
        try
        {
            var response = await _httpClient.GetAsync("http://127.0.0.1:1234/v1/models");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetBerthRecommendation(int vesselId, string context)
    {
        var prompt = $@"You are a port operations AI assistant. Based on the following port and vessel information, recommend the best berth assignment and explain why.

            Context: {context}

            Provide a concise recommendation (2-3 sentences) focusing on:
            1. Which berth to assign
            2. Key factors (vessel size, draft, cargo type, berth availability)
            3. Any potential concerns

            Recommendation:";

        return await CallLmStudio(prompt);
    }

    public async Task<string> AnswerQuestion(string question, string context)
    {
        var prompt = $@"You are a port operations AI assistant. Answer the following question about port operations based on the provided context.

            Context: {context}

            Question: {question}

            Provide a clear, concise answer (2-4 sentences):";

        return await CallLmStudio(prompt);
    }

    public async Task<string> GenerateRealisticPortCallScenario()
    {
        var prompt = @"Generate a realistic port call scenario in JSON format with the following structure:
            {
                ""vesselName"": ""unique realistic vessel name"",
                ""imoNumber"": ""IMO followed by 7 digits"",
                ""vesselType"": ""Container|Tanker|BulkCarrier|RoRo|Cruise"",
                ""flagCountry"": ""country name"",
                ""cargoDescription"": ""specific cargo type"",
                ""cargoQuantity"": numeric value,
                ""cargoUnit"": ""TEU|tons|m³"",
                ""delayReason"": ""realistic delay reason or null"",
                ""priorityLevel"": 1-5
            }

            Generate ONE scenario. Return ONLY valid JSON, no explanation:";

        return await CallLmStudio(prompt, maxTokens: 300);
    }

    private async Task<string> CallLmStudio(string prompt, int maxTokens = 200)
    {
        try
        {
            var request = new
            {
                model = "qwen2.5-7b-instruct",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant specialized in maritime port operations." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = maxTokens
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Calling LM Studio API...");
            var response = await _httpClient.PostAsync(LmStudioUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("LM Studio returned {StatusCode}", response.StatusCode);
                return "AI service unavailable";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var result = JsonDocument.Parse(responseJson);

            var message = result.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return message ?? "No response from AI";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling LM Studio");
            return $"AI Error: {ex.Message}";
        }
    }
}
