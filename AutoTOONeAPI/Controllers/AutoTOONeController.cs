using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AutoTOONeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToonController : ControllerBase
{
    /// <summary>
    /// Converte JSON para formato TOON
    /// </summary>
    [HttpPost("convert")]
    [Consumes("application/json", "text/plain")]
    [Produces("text/plain")]
    public IActionResult ConvertToToon([FromBody] JsonElement jsonInput)
    {
        try
        {
            string toonResult = ToonConverter.ConvertJsonToToon(jsonInput);
            return Ok(toonResult);
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
    /// Converte JSON a partir de arquivo enviado
    /// </summary>
    [HttpPost("convert/file")]
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
            
            return Ok(toonResult);
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
    
    /// <summary>
    /// Converte JSON para TOON e retorna arquivo .txt para download
    /// </summary>
    [HttpPost("convert/download")]
    [Consumes("application/json", "text/plain")]
    public IActionResult ConvertAndDownload([FromBody] JsonElement jsonInput)
    {
        try
        {
            string toonResult = ToonConverter.ConvertJsonToToon(jsonInput);
            byte[] bytes = Encoding.UTF8.GetBytes(toonResult);
            
            return File(bytes, "text/plain", $"output_{DateTime.Now:yyyyMMdd_HHmmss}.toon.txt");
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
}