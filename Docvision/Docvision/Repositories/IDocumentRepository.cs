using Docvision.Models;
using Microsoft.AspNetCore.Identity;

namespace Docvision.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> GetDocumentByIdAsync(Guid Id,string userId);
        Task<List<Document>> GetAllDocumentAsync(string userId);
        Task<Document> AddDocumentAsync(IFormFile file, string userId);
        Task<Document> UpdateDocumentAsync(Guid Id, Document document,string userId);
        Task<Document> GetDocumentByIdAsync(Guid Id, string userId, bool includeTracking = false);
        Task<bool> SaveChangesAsync();

        Task<Document> DeleteDocumentAsync(Guid Id, string userId);
        Task<Stream> GetDocumentStreamAsync(Guid id);
        Task<string> ExtractTextFromPdfAsync(Guid documentId);
        Task<List<DocumentImage>> ExtractAndSaveImagesFromPdfAsync(Guid documentId, string userId);


    }
}
