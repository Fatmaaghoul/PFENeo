using Docvision.Models;

namespace Docvision.Services
{
    public interface IDocumentService
    {
        Task<List<Document>> GetAllDocumentsAsync();
        Task<Document?> GetDocumentByIdAsync(Guid id);
        Task<Document> AddDocumentAsync(Document document);
        Task<Document?> UpdateDocumentAsync(Guid id, string? name, string? description);
        Task<bool> DeleteDocumentAsync(Guid id);
        Task<ApplicationUser?> GetUserByDocumentIdAsync(Guid documentId);
    }
}
