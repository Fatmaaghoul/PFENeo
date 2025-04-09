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


namespace Back.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _idocumentRepository;
        private DocContext _context;
        
        public DocumentController(IDocumentRepository IdocumentRepository , DocContext docContext)
        {
            _idocumentRepository = IdocumentRepository;
            _context = docContext;
          
        }


        [HttpPost("add")]
        [Consumes("multipart/form-data")]  
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }
               var document = await _idocumentRepository.AddDocumentAsync(file, userId);

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
            var document = await _idocumentRepository.GetDocumentByIdAsync(id,userId);            
            return Ok(document);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] Document updatedDocument)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }
           var document = await _idocumentRepository.UpdateDocumentAsync(id,updatedDocument,userId);

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
           await _idocumentRepository.DeleteDocumentAsync(id,userId);

            return Ok("Document supprimé avec succès.");
        }

    }
}
