using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using AdvertisingPlatforms;

namespace AdvertisingPlatforms.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisingController(Tree tree) : ControllerBase
    {
        private readonly Tree _tree = tree;

        // [Authorize] // TODO: критическую инфраструктуру желательно хоть немного защищать. Хотя бы JWT токеном в заголовке.
        [HttpPost("load")]
        public async Task<IActionResult> Load()
        {
            if (!Request.HasFormContentType || !Request.Form.Files.Any())
                return BadRequest(new { message = "No file uploaded." });

            var file = Request.Form.Files[0];
            if (file.Length == 0)
                return BadRequest(new { message = "Empty file uploaded." });

            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                var lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

                _tree.Reload(lines);
                return Ok(new { message = "Data loaded successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to load data: {ex.Message}" });
            }
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return BadRequest(new { message = "Location is required." });

            var platforms = _tree.Search(location);
            return platforms != null
                ? Ok(new { platforms })
                : NotFound(new { message = "Location not found." });
        }
    }
}