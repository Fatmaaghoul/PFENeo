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

        Task<Document> DeleteDocumentAsync(Guid Id, string userId);

       
    }
}
