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


namespace Back.Controllers
{
    [ApiController]
    [Route("api/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;
        private readonly DocContext _context;

        public DocumentController(Cloudinary cloudinary, DocContext context)
        {
            _cloudinary = cloudinary;
            _context = context;
        }


        [HttpPost("add")]
        [Consumes("multipart/form-data")]  // Ajoutez cette ligne
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            try
            {
               // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
          //  if (string.IsNullOrEmpty(userId))
           // {
           //     return Unauthorized("Utilisateur non authentifié.");
          //  }
                if (file == null || file.Length == 0)
                    return BadRequest("Aucun fichier sélectionné.");

                using var stream = file.OpenReadStream();

                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "documents",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    return BadRequest($"Erreur Cloudinary : {uploadResult.Error.Message}");

                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    Name = file.FileName,
                    UploadDate = DateTime.UtcNow,
                    FileUrl = uploadResult.SecureUrl.ToString(),
                    UserId = "c3cfcd42-689f-4c50-8a4b-5f3bff2f469f",
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

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
            var documents = await _context.Documents.ToListAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound("Document non trouvé.");

            return Ok(document);
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDocumentsByUserId(Guid userId)
        {
            try
            {
                var documents = await _context.Documents
                    .Where(d => d.UserId == userId.ToString()) // Comparer les deux en tant que string.
                    .ToListAsync();

                if (documents == null || documents.Count == 0)
                    return NotFound($"Aucun document trouvé pour l'utilisateur avec l'ID {userId}");

                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] Document updatedDocument)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound("Document non trouvé.");

            document.Name = updatedDocument.Name ?? document.Name;
            document.Text = updatedDocument.Text ?? document.Text;

            _context.Documents.Update(document);
            await _context.SaveChangesAsync();

            return Ok(document);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound("Document non trouvé.");
            // Suppression de la base de données
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok("Document supprimé avec succès.");
        }

    }
}
