using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docvision.Persistance;
using Docvision.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Docvision.Repositories;
using Docvision.Dtos;


namespace Back.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _idocumentRepository;
        private DocContext _context;

        public DocumentController(IDocumentRepository IdocumentRepository, DocContext docContext)
        {
            _idocumentRepository = IdocumentRepository;
            _context = docContext;

        }


        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(IFormFile file,string description)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }
                var document = await _idocumentRepository.AddDocumentAsync(file, description, userId);

                return Ok(document);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de l'upload du document : {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }
                var documents = await _idocumentRepository.GetAllDocumentAsync(userId);

                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }
            var document = await _idocumentRepository.GetDocumentByIdAsync(id, userId);
            return Ok(document);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] DocumentUpdateDto updatedDocument)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            var document = await _idocumentRepository.UpdateDocumentAsync(id, updatedDocument, userId);

            if (document == null)
            {
                return NotFound("Document non trouvé ou l'utilisateur n'a pas accès à ce document.");
            }

            return Ok(document);
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }
            await _idocumentRepository.DeleteDocumentAsync(id, userId);

            return Ok("Document supprimé avec succès.");
        }
        [HttpPost("extract/{id}")]
        public async Task<IActionResult> Extract(Guid id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null) return NotFound();

            var requestBody = new { pdf_url = document.FileUrl };

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync("http://127.0.0.1:8000/extract", requestBody);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Erreur appel FastAPI");

            var result = await response.Content.ReadFromJsonAsync<ExtractedResult>();
            if (result == null)
                return StatusCode(500, "Réponse invalide");

            // Mise à jour du texte
            document.Text = result.text;
            document.isExtracted = true;

            // Supprimer les anciennes images liées au document
            var existingImages = await _context.Images.Where(i => i.DocumentId == document.Id).ToListAsync();
            _context.Images.RemoveRange(existingImages);

            // Ajouter les nouvelles images
            foreach (var imageUrl in result.images)
            {
                var image = new DocumentImage
                {
                    Id = Guid.NewGuid(),
                    FileUrl = imageUrl,
                    DocumentId = document.Id
                };
                _context.Images.Add(image);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Extraction réussie",
                text = document.Text,
                images = result.images
            });
        }
        public class ExtractedResult
        {
            public string text { get; set; } = "";
            public List<string> images { get; set; } = new();
        }


    }
}
