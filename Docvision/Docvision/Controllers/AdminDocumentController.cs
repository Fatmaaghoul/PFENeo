using Docvision.Models;
using Docvision.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Docvision.Controllers
{
    [ApiController]
    [Route("api/admin/documents")]
  
    public class DocumentAdminController : ControllerBase
    {
       
        private readonly IDocumentService _documentService;

        public DocumentAdminController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // ✅ GET: All documents
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
        //   if (!IsAdmin()) return Forbid();

            var docs = await _documentService.GetAllDocumentsAsync();
            return Ok(docs);
        }

        // ✅ GET: Document by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
           // if (!IsAdmin()) return Forbid();

            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            return Ok(doc);
        }

        // ✅ POST: Create a document
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Document doc)
        {
          //  if (!IsAdmin()) return Forbid();

            var created = await _documentService.AddDocumentAsync(doc);
            return Ok(created);
        }

        // ✅ PUT: Update document name/description
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequest model)
        {
           // if (!IsAdmin()) return Forbid();

            var updated = await _documentService.UpdateDocumentAsync(id, model.Name, model.Description);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        // ✅ DELETE: Remove a document
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
          //  if (!IsAdmin()) return Forbid();

            var deleted = await _documentService.DeleteDocumentAsync(id);
            if (!deleted) return NotFound();

            return Ok("Document supprimé avec succès.");
        }

        // ✅ GET: User by DocumentId
        [HttpGet("{id}/user")]
        public async Task<IActionResult> GetUserByDocumentId(Guid id)
        {
          //  if (!IsAdmin()) return Forbid();

            var user = await _documentService.GetUserByDocumentIdAsync(id);
            if (user == null) return NotFound("Utilisateur non trouvé pour ce document.");

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                
            });
        }

        public class UpdateDocumentRequest
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
    }
}
