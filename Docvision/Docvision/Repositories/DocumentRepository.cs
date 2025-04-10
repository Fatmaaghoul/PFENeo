﻿using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Docvision.Models;
using Microsoft.EntityFrameworkCore;
using Docvision.Persistance;
using Docvision.Dtos;

namespace Docvision.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly Cloudinary _cloudinary;
        private readonly DocContext _Context;
        public DocumentRepository(Cloudinary cloudinary, DocContext docContext)
        {
            _cloudinary = cloudinary;
            _Context = docContext;
        }

        public async Task<Document> AddDocumentAsync(IFormFile file,string description, string userId)
        {
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

            var document = new Document
            {
                Id = Guid.NewGuid(),
                Name = file.FileName,
                UploadDate = DateTime.UtcNow,
                FileUrl = uploadResult.SecureUrl.ToString(),
                UserId = userId,
                description = description,
            };

            _Context.Documents.Add(document);
            await _Context.SaveChangesAsync();

            return document;
        }

        public async Task<Document?> DeleteDocumentAsync(Guid Id, string userId)
        {
            var document = await _Context.Documents.FirstOrDefaultAsync(d => d.Id == Id && d.UserId == userId);
            if (document == null) return null;

            var publicId = document.FileUrl.Split('/').Last().Split('.')[0];
            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);

            _Context.Documents.Remove(document);
            await _Context.SaveChangesAsync();

            return document;
        }

        public async Task<List<Document>> GetAllDocumentAsync(string userId)
        {
            return await _Context.Documents
                                 .Where(d => d.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid Id, string userId)
        {
            return await _Context.Documents.FirstOrDefaultAsync(d => d.Id == Id && d.UserId == userId);
        }

        public async Task<Document?> UpdateDocumentAsync(Guid id, DocumentUpdateDto updatedDocument, string userId)
        {
            var document = await _Context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
            {
                return null; // Si aucun document n'est trouvé, on retourne null
            }

            // Mise à jour uniquement du nom et de la description, si spécifié
            if (!string.IsNullOrEmpty(updatedDocument.Name))
            {
                document.Name = updatedDocument.Name;
            }

            if (!string.IsNullOrEmpty(updatedDocument.Description))
            {
                document.description = updatedDocument.Description;
            }

            _Context.Documents.Update(document);
            await _Context.SaveChangesAsync();

            return document;
        }


    }
}
