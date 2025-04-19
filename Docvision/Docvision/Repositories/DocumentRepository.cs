using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Docvision.Models;
using Docvision.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using UglyToad.PdfPig;

namespace Docvision.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly Cloudinary _cloudinary;
        private readonly DocContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DocumentRepository> _logger;

        public DocumentRepository(
            Cloudinary cloudinary,
            DocContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<DocumentRepository> logger)
        {
            _cloudinary = cloudinary;
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<Document> AddDocumentAsync(IFormFile file, string userId)
        {
            await using var stream = file.OpenReadStream();

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
                UserId = userId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        public async Task<Document?> DeleteDocumentAsync(Guid id, string userId)
        {
            // Récupérer le document avec ses images
            var document = await _context.Documents
                .Include(d => d.Images)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
                return null;

            try
            {
                // Supprimer d'abord les images de Cloudinary
                if (document.Images != null)
                {
                    foreach (var image in document.Images)
                    {
                        try
                        {
                            var publicId = GetCloudinaryPublicId(image.FileUrl);
                            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erreur lors de la suppression de l'image {ImageId} de Cloudinary", image.Id);
                        }
                    }
                }

                // Supprimer le document de Cloudinary
                try
                {
                    var docPublicId = GetCloudinaryPublicId(document.FileUrl);
                    await _cloudinary.DestroyAsync(new DeletionParams(docPublicId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la suppression du document {DocumentId} de Cloudinary", id);
                }

                // La suppression en cascade s'occupera des images en base
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du document {DocumentId}", id);
                throw;
            }
        }
        public async Task<List<Document>> GetAllDocumentAsync(string userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.Images) // Charge les images associées
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid id, string userId)
        {
            return await _context.Documents
                        .Include(d => d.Images) // Explicitly load images

                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        }


        public async Task<Document> GetDocumentByIdAsync(Guid id, string userId, bool includeTracking = false)
        {
            var query = _context.Documents
                .Include(d => d.Images)
                .Where(d => d.Id == id && d.UserId == userId);

            if (includeTracking)
            {
                query = query.AsTracking();
            }
            else
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Stream> GetDocumentStreamAsync(Guid id)
        {
            var document = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
                throw new FileNotFoundException("Document not found");

            if (string.IsNullOrWhiteSpace(document.FileUrl))
                throw new InvalidOperationException("Document URL is empty");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                var response = await httpClient.GetAsync(
                    document.FileUrl,
                    HttpCompletionOption.ResponseHeadersRead);

                response.EnsureSuccessStatusCode();

                var memoryStream = new MemoryStream();
                await response.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error downloading document from {Url}", document.FileUrl);
                throw new Exception("Could not download document", ex);
            }
        }

        public async Task<Document?> UpdateDocumentAsync(Guid id, Document updatedDocument, string userId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
                return null;

            document.Name = updatedDocument.Name ?? document.Name;
            document.Text = updatedDocument.Text ?? document.Text;

            await _context.SaveChangesAsync();

            return document;
        }
        public async Task<string> ExtractTextFromPdfAsync(Guid documentId)
        {
            await using var stream = await GetDocumentStreamAsync(documentId);

            using (var document = PdfDocument.Open(stream))
            {
                var text = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return text.ToString();
            }
        }

        public async Task<List<DocumentImage>> ExtractAndSaveImagesFromPdfAsync(Guid documentId, string userId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new FileNotFoundException("Document not found");

            await using var stream = await GetDocumentStreamAsync(documentId);
            var images = new List<DocumentImage>();
            int imageCount = 0;

            using (var pdfDocument = PdfDocument.Open(stream))
            {
                foreach (var page in pdfDocument.GetPages())
                {
                    foreach (var image in page.GetImages())
                    {
                        imageCount++;
                        try
                        {
                            var bytes = image.RawBytes.ToArray();
                            if (bytes.Length > 0)
                            {
                                var imageUrl = await UploadImageToCloudinary(bytes, $"doc_{documentId}_img_{imageCount}");

                                var documentImage = new DocumentImage
                                {
                                    Id = Guid.NewGuid(),
                                    FileUrl = imageUrl,
                                    Description = $"Image {imageCount} from document",
                                    DocumentId = documentId
                                };

                                _context.Images.Add(documentImage);
                                images.Add(documentImage);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing image {imageCount}");
                        }
                    }
                }
            }

            // Mettez à jour les indicateurs d'extraction
            document.ImagesExtracted = true;
            document.ImagesExtractedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return images;
        }
        private async Task<string> UploadImageToCloudinary(byte[] imageData, string publicId)
        {
            using var stream = new MemoryStream(imageData);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(publicId, stream),
                PublicId = publicId,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }

        private string GetCloudinaryPublicId(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("URL cannot be null or empty");

            try
            {
                var uri = new Uri(fileUrl);
                var lastSegment = uri.Segments.Last();
                return Path.GetFileNameWithoutExtension(lastSegment);
            }
            catch (UriFormatException ex)
            {
                _logger.LogError(ex, "Invalid Cloudinary URL format: {Url}", fileUrl);
                throw new InvalidOperationException("Invalid Cloudinary URL format", ex);
            }
        }
    }
}