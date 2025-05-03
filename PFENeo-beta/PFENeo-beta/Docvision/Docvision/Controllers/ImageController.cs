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




}
}
