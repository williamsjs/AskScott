using Microsoft.AspNetCore.Mvc;
using AskProject.Api.Services;

namespace AskProject.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AskController : ControllerBase
{
    private readonly HuggingFaceService _huggingFaceService;
    private readonly string _apiKey;

    public AskController(HuggingFaceService huggingFaceService, IConfiguration configuration)
    {
        _huggingFaceService = huggingFaceService;
        _apiKey = configuration["ApiKey"];
    }

    [HttpPost("AskQuestion")]
    public async Task<ActionResult<string>> AskQuestion([FromHeader(Name = "X-API-KEY")] string apiKey, [FromBody] string question)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey != _apiKey)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _huggingFaceService.GenerateCompletion(question);
            return Ok(response);
        }
        catch
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}