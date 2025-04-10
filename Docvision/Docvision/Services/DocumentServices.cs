using Docvision.Models;
using Docvision.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Docvision.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly DocContext _context;

        public DocumentService(DocContext context)
        {
            _context = context;
        }

        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            return await _context.Documents.ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<Document> AddDocumentAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<Document?> UpdateDocumentAsync(Guid id, string? name, string? description)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return null;

            if (!string.IsNullOrEmpty(doc.Name))
            {
                doc.Name = name;
            }

            if (!string.IsNullOrEmpty(doc.description))
            {
                doc.description = description;
            }

            _context.Documents.Update(doc);
            await _context.SaveChangesAsync();

            return doc;
        }

        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            var doc = await _context.Documents.Include(d => d.Images).FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return false;

            // Supprimer les images associées
            _context.Images.RemoveRange(doc.Images);

            // Supprimer le document
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
    }

        public async Task<ApplicationUser?> GetUserByDocumentIdAsync(Guid documentId)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null) return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == document.UserId);
        }
    }
}
