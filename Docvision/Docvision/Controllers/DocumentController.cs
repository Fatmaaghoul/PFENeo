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
using Docvision.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _idocumentRepository;
        private readonly DocContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<DocumentRepository> _logger;


        public DocumentController(IDocumentRepository IdocumentRepository, DocContext docContext, IHttpClientFactory httpClientFactory, ILogger<DocumentRepository> logger)
        {
            _idocumentRepository = IdocumentRepository;
            _context = docContext;
            _httpClient = httpClientFactory.CreateClient(nameof(DocumentController)); // Utiliser le client nommé
            _logger = logger;

        }

        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] CreateDocumentRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }
                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    ModelState.AddModelError("Description", "The description field is required.");
                }
                var document = await _idocumentRepository.AddDocumentAsync(request.File,request.Name, request.Description, userId);

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
                var result = documents.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    UploadDate = d.UploadDate,
                    Description = d.description,
                    IsAnalysed = d.isAnalysed,
                    IsExtracted = d.isExtracted,
                    FileUrl = d.FileUrl,
                    Text = d.Text,
                    Images = d.Images?.Select(i => new DocumentImageDto
                    {
                        Id = i.Id,
                        FileUrl = i.FileUrl,
                        Description = i.Description,
                        Objects = i.Objects
                    }).ToList() ?? new List<DocumentImageDto>()
                }).ToList();

                return Ok(result);
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

            var document = await _idocumentRepository.GetDocumentByIdAsync(id, userId);
            if (document == null) return NotFound();

            var result = new DocumentDto
            {
                Id = document.Id,
                Name = document.Name,
                UploadDate = document.UploadDate,
                Description = document.description,
                IsAnalysed = document.isAnalysed,
                IsExtracted = document.isExtracted,
                FileUrl = document.FileUrl,
                Text = document.Text,
                Images = document.Images?.Select(i => new DocumentImageDto
                {
                    Id = i.Id,
                    FileUrl = i.FileUrl,
                    Description = i.Description,
                    Objects = i.Objects
                }).ToList() ?? new List<DocumentImageDto>()
            };

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] DocumentUpdateDto

 updatedDocument)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            var document = await _idocumentRepository.UpdateDocumentAsync(id, updatedDocument, userId);
            if (document == null)
            {
                return NotFound("Document non trouvé ou l'utilisateur n'a pas accès à ce document.");
            }

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
            await _idocumentRepository.DeleteDocumentAsync(id, userId);

            return Ok("Document supprimé avec succès.");
        }
        [HttpPost("extract/{id}")]
        public async Task<IActionResult> Extract(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            try
            {
                // Extraction texte + images (sans analyse)
                var result = await _idocumentRepository.ExtractTextAndImagesAsync(document.FileUrl);

                // Mise à jour du document
                document.Text = result.Text;
                document.isExtracted = true; // Marquer comme extrait
                document.isAnalysed = false;  // Aucune analyse effectuée

                // Remplacer les anciennes images
                var oldImages = await _context.Images.Where(i => i.DocumentId == id).ToListAsync();
                _context.Images.RemoveRange(oldImages);

                foreach (var imageUrl in result.ImageUrls)
                {
                    _context.Images.Add(new DocumentImage
                    {
                        Id = Guid.NewGuid(),
                        FileUrl = imageUrl,
                        DocumentId = id,
                        Description = "Image extraite", 
                        Objects = null 
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Extraction texte+images réussie",
                    Text = result.Text,
                    Images = result.ImageUrls
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur d'extraction");
                return StatusCode(500, "Erreur lors de l'extraction");
            }
        }

        /*  [HttpPost("extract/{id}")]
          public async Task<IActionResult> Extract(Guid id)
          {
              var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
              if (document == null) return NotFound();

              var requestBody = new { pdf_url = document.FileUrl };

              var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/extract", requestBody);
              if (!response.IsSuccessStatusCode)
                  return StatusCode((int)response.StatusCode, "Erreur appel FastAPI");

              var result = await response.Content.ReadFromJsonAsync<ExtractedResult>();
              if (result == null)
                  return StatusCode(500, "Réponse invalide");

              // Mise à jour du texte
              document.Text = result.text;
              document.isExtracted = true;

              // Supprimer les anciennes images liées au document
              var existingImages = await _context.Images.Where(i => i.DocumentId == document.Id).ToListAsync();
              _context.Images.RemoveRange(existingImages);

              // Ajouter les nouvelles images
              foreach (var imageUrl in result.images)
              {
                  var image = new DocumentImage
                  {
                      Id = Guid.NewGuid(),
                      FileUrl = imageUrl,
                      DocumentId = document.Id
                  };
                  _context.Images.Add(image);
              }

              await _context.SaveChangesAsync();

              return Ok(new
              {
                  message = "Extraction réussie",
                  text = document.Text,
                  images = result.images
              });
          }*/

        [HttpPost("describe/{id}")]
        public async Task<IActionResult> DescribeImages(Guid id)
        {
            try
            {
                // Récupérer le document
                var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
                if (document == null)
                    return NotFound("Document non trouvé.");

                if (string.IsNullOrEmpty(document.Text))
                    return BadRequest("Le document n'a pas de texte extrait. Veuillez d'abord extraire le document.");

                // Récupérer les images associées
                var images = await _context.Images.Where(i => i.DocumentId == id).ToListAsync();
                if (!images.Any())
                    return NotFound("Aucune image trouvée pour ce document.");

                // Traiter les images en parallèle
                var tasks = images.Select(async image =>
                {
                    try
                    {
                        // Valider l'URL de l'image
                        if (string.IsNullOrEmpty(image.FileUrl) || !Uri.IsWellFormedUriString(image.FileUrl, UriKind.Absolute))
                        {
                            return new
                            {
                                ImageId = image.Id,
                                Success = false,
                                Error = $"URL de l'image {image.FileUrl} invalide."
                            };
                        }

                        var requestBody = new
                        {
                            image_url = image.FileUrl,
                            text = document.Text
                        };

                        var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/describe_image", requestBody);
                        if (!response.IsSuccessStatusCode)
                        {
                            return new
                            {
                                ImageId = image.Id,
                                Success = false,
                                Error = $"Erreur lors de l'appel à l'API FastAPI pour l'image {image.FileUrl}: {(int)response.StatusCode}"
                            };
                        }

                        var result = await response.Content.ReadFromJsonAsync<DescribeImageResult>();
                        if (result == null)
                        {
                            return new
                            {
                                ImageId = image.Id,
                                Success = false,
                                Error = $"Réponse invalide de l'API pour l'image {image.FileUrl}"
                            };
                        }

                        // Mettre à jour l'image
                        image.Description = result.description;
                        image.Objects = JsonSerializer.Serialize(result.detected_objects);
                        document.isAnalysed = true;

                        return new
                        {
                            ImageId = image.Id,
                            Success = true,
                            Error = (string?)null
                        };
                    }
                    catch (TaskCanceledException)
                    {
                        return new
                        {
                            ImageId = image.Id,
                            Success = false,
                            Error = $"La requête pour l'image {image.FileUrl} a expiré."
                        };
                    }
                    catch (HttpRequestException ex)
                    {
                        return new
                        {
                            ImageId = image.Id,
                            Success = false,
                            Error = $"Erreur réseau lors du traitement de l'image {image.FileUrl}: {ex.Message}"
                        };
                    }
                    catch (Exception ex)
                    {
                        return new
                        {
                            ImageId = image.Id,
                            Success = false,
                            Error = $"Erreur inattendue lors du traitement de l'image {image.FileUrl}: {ex.Message}"
                        };
                    }
                });

                // Attendre que toutes les requêtes soient terminées
                var results = await Task.WhenAll(tasks);

                // Sauvegarder les modifications pour les images traitées avec succès
                await _context.SaveChangesAsync();

                // Vérifier s'il y a des erreurs
                var errors = results.Where(r => !r.Success).ToList();
                if (errors.Any())
                {
                    return Ok(new
                    {
                        Message = "Certaines images n'ont pas pu être traitées, mais les images valides ont été enregistrées.",
                        Results = results.Select(r => new
                        {
                            r.ImageId,
                            r.Success,
                            r.Error
                        })
                    });
                }

                // Retourner les détails des images traitées
                return Ok(new
                {
                    Message = "Toutes les images ont été traitées et enregistrées avec succès.",
                    Images = images.Select(i => new
                    {
                        i.Id,
                        i.FileUrl,
                        i.Description,
                        Objects = string.IsNullOrEmpty(i.Objects) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(i.Objects)
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }
        public class ExtractedResult
        {
            public string Text { get; set; }  
            public List<string> ImageUrls { get; set; }  
        }

        public class DescribeImageResult
        {
            public List<string> detected_objects { get; set; } = new();
            public List<string> mentioned_objects { get; set; } = new();
            public List<string> objects_to_describe { get; set; } = new();
            public string description { get; set; } = "";
        }

        public class CreateDocumentRequest
        {
            [Required]
            public IFormFile File { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Description { get; set; }
            
        }
    }
}