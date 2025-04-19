using Docvision.Dtos;
using Docvision.Persistance;
using Docvision.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace doc.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : Controller
    {
        private readonly DocContext _docContext;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<ImageController> _logger;

        public ImageController(
            DocContext docContext,
            IDocumentRepository documentRepository,
            ILogger<ImageController> logger)
        {
            _docContext = docContext;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        [HttpGet("{docId}")]
        public async Task<IActionResult> GetAllImages(Guid docId)
        {
            // Vérifie d'abord si des images existent déjà en base
            var existingImages = await _docContext.Images
                .Where(i => i.DocumentId == docId)
                .ToListAsync();

            if (existingImages.Any())
            {
                return Ok(existingImages);
            }

            // Si aucune image en base, essaie d'extraire depuis le PDF
            try
            {
                var extractedImages = await _documentRepository.ExtractAndSaveImagesFromPdfAsync(docId, GetUserId());

                if (extractedImages == null || !extractedImages.Any())
                {
                    return NotFound("Aucune image trouvée dans le document");
                }

                return Ok(extractedImages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting images from document {docId}");
                return StatusCode(500, "Une erreur est survenue lors de l'extraction des images");
            }
        }

        [HttpPost("{docId}/extract")]
        public async Task<IActionResult> ExtractImagesFromPdf(Guid docId)
        {
            try
            {
                // Supprime les anciennes images associées au document
                var oldImages = await _docContext.Images
                    .Where(i => i.DocumentId == docId)
                    .ToListAsync();

                if (oldImages.Any())
                {
                    _docContext.Images.RemoveRange(oldImages);
                    await _docContext.SaveChangesAsync();
                }

                // Extrait et sauvegarde les nouvelles images
                var extractedImages = await _documentRepository.ExtractAndSaveImagesFromPdfAsync(docId, GetUserId());

                return Ok(new ImageExtractionResultDto
                {
                    Message = "Images extraites avec succès",
                    Count = extractedImages.Count,
                    Images = extractedImages.Select(i => new ImageDto
                    {
                        Id = i.Id,
                        FileUrl = i.FileUrl,
                        Description = i.Description
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting images from document {docId}");
                return StatusCode(500, "Erreur lors de l'extraction des images");
            }
        }

        private string GetUserId()
        {
            // Implémentez votre logique de récupération d'userId
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
