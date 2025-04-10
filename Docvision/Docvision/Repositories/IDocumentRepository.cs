using Docvision.Dtos;
using Docvision.Models;
using Microsoft.AspNetCore.Identity;

namespace Docvision.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> GetDocumentByIdAsync(Guid Id,string userId);
        Task<List<Document>> GetAllDocumentAsync(string userId);
        Task<Document> AddDocumentAsync(IFormFile file,string description, string userId);
        Task<Document> UpdateDocumentAsync(Guid Id, DocumentUpdateDto updatedDocument, string userId);

        Task<Document> DeleteDocumentAsync(Guid Id, string userId);

       
    }
}
