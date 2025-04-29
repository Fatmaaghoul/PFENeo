using Docvision.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace doc.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : Controller
    {
        private readonly DocContext _docContext;
        private readonly HttpClient _httpClient;
        public ImageController(DocContext docContext, HttpClient httpClient)
        {
            _docContext = docContext;
            _httpClient = httpClient;
        }

        [HttpGet("{docId}")] 
        public async Task<IActionResult> GetAllImages(Guid docId)
        {
            
            var images = await _docContext.Images
                .Where(i => i.DocumentId == docId)
                .ToListAsync();

            if (images == null || !images.Any())
            {
                return NotFound("No images found for the given document ID");
            }

            return Ok(images);
        }

        [HttpPost("{imageId}/caption")]
        public async Task<IActionResult> GenerateCaption(Guid imageId)
        {
            try
            {
                // Retrieve the image from the database
                var image = await _docContext.Images
                    .FirstOrDefaultAsync(i => i.Id == imageId);

                if (image == null)
                {
                    return NotFound("Image not found for the given ID");
                }

                // Prepare the request body for the external API
                var requestBody = new { image_url = image.FileUrl };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Call the external /caption API
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/caption", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Failed to generate caption: {errorContent}");
                }

                // Parse the API response
                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("error", out var error))
                {
                    return BadRequest($"API error: {error.GetString()}");
                }

                if (!root.TryGetProperty("caption", out var captionElement))
                {
                    return BadRequest("No caption returned from the API");
                }

                var caption = captionElement.GetString();

                // Update the image with the caption
                image.Caption = caption;
                await _docContext.SaveChangesAsync();

                // Return the caption
                return Ok(new { caption });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    



}
}
