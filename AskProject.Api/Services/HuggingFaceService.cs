using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AskProject.Api.Services;

public class HuggingFaceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuggingFaceService> _logger;

    public HuggingFaceService(HttpClient httpClient, IConfiguration configuration, ILogger<HuggingFaceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var apiKey = configuration["HuggingFace:ApiKey"] ?? 
            throw new ArgumentNullException("HuggingFace:ApiKey configuration is missing");
            
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GenerateCompletion(string prompt)
    {
        try
        {
            // You can change the model here based on your preference
            string modelEndpoint = "https://api-inference.huggingface.co/models/google/flan-t5-base";
            
            var payload = new { inputs = prompt };
            
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Sending request to Hugging Face API with prompt: {Prompt}", prompt);
            var response = await _httpClient.PostAsync(modelEndpoint, content);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Hugging Face API error: {StatusCode}, {Response}", 
                    response.StatusCode, responseBody);
                throw new Exception($"API error: {response.StatusCode}, {responseBody}");
            }

            _logger.LogInformation("Received response from Hugging Face API");
            
            // The response format depends on the model used
            using var jsonDoc = JsonDocument.Parse(responseBody);
            
            // For text generation models
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return jsonDoc.RootElement[0].GetProperty("generated_text").GetString() ?? 
                    "No response generated";
            }
            
            // For chat models
            return responseBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating completion from Hugging Face API");
            throw;
        }
    }
}