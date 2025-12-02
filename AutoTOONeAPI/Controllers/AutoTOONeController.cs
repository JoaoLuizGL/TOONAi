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

    /// <summary>
    /// Endpoint de teste
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "OK", message = "TOON Converter API está funcionando" });
    }
}