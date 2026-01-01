using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AutoTOONeAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ToonController : ControllerBase
{
    /// <summary>
    /// Converte JSON para formato TOON (recebe JSON no body)
    /// </summary>
    [HttpPost("convert")]
    [Consumes("application/json")]
    [Produces("text/plain")]
    public IActionResult ConvertToToon([FromBody] JsonElement jsonInput)
    {
        try
        {
            string toonResult = ToonConverter.ConvertJsonToToon(jsonInput);
            return Content(toonResult, "text/plain");
        }
        catch (JsonException ex)
        {
            return BadRequest(new { error = "JSON inválido", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao processar conversão", message = ex.Message });
        }
    }

    /// <summary>
    /// Converte JSON para formato TOON (recebe arquivo .json)
    /// </summary>
    [HttpPost("convert/file")]
    [Produces("text/plain")]
    public async Task<IActionResult> ConvertFromFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Nenhum arquivo enviado" });

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            string jsonContent = await reader.ReadToEndAsync();
            
            JsonDocument doc = JsonDocument.Parse(jsonContent);
            string toonResult = ToonConverter.ConvertJsonToToon(doc.RootElement);
            
            return Content(toonResult, "text/plain");
        }
        catch (JsonException ex)
        {
            return BadRequest(new { error = "JSON inválido no arquivo", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao processar arquivo", message = ex.Message });
        }
    }
}