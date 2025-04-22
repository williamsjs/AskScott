using Microsoft.AspNetCore.Mvc;
using AskProject.Api.Services;

namespace AskProject.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AskController : ControllerBase
{
    private readonly HuggingFaceService _huggingFaceService;

    public AskController(HuggingFaceService huggingFaceService)
    {
        _huggingFaceService = huggingFaceService;
    }

    [HttpPost("AskQuestion")]
    public async Task<ActionResult<string>> AskQuestion([FromBody] string question)
    {
        try
        {
            var response = await _huggingFaceService.GenerateCompletion(question);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}