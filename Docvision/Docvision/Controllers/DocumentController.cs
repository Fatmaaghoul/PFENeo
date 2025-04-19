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
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Docvision.Dtos;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DocumentController> _logger;
        private const string PythonServiceUrl = "http://localhost:5000/analyze";

        public DocumentController(
            IDocumentRepository documentRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<DocumentController> logger)
        {
            _documentRepository = documentRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // [POST] api/documents/add - CONSERVE L'EXISTANT
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }

                var document = await _documentRepository.AddDocumentAsync(file, userId);
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload du document");
                return BadRequest($"Erreur lors de l'upload : {ex.Message}");
            }
        }

        // [GET] api/documents - CONSERVE L'EXISTANT
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }

                var documents = await _documentRepository.GetAllDocumentAsync(userId);

                // Transformation des documents en DTO avec images
                var result = documents.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    UploadDate = d.UploadDate,
                    FileUrl = d.FileUrl,
                    Text = d.Text,
                    UserId = d.UserId,
                    ImagesExtracted = d.ImagesExtracted,
                    ImagesExtractedDate = d.ImagesExtractedDate,
                    Images = d.Images?.Select(i => new ImageDto
                    {
                        Id = i.Id,
                        FileUrl = i.FileUrl,
                        Description = i.Description
                    }).ToList() ?? new List<ImageDto>()
                }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents");
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }
        /*public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }
                var documents = await _documentRepository.GetAllDocumentAsync(userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents");
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }*/


        // [GET] api/documents/{id} - CONSERVE L'EXISTANT
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            var document = await _documentRepository.GetDocumentByIdAsync(id, userId);
            if (document == null)
            {
                return NotFound();
            }

            // Nouveau: Extraction des mainObjects depuis les Metadata des images
            var mainObjects = new List<string>();
            if (document.Images != null)
            {
                foreach (var image in document.Images)
                {
                    if (!string.IsNullOrEmpty(image.Metadata))
                    {
                        try
                        {
                            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(image.Metadata);
                            if (metadata != null && metadata.TryGetValue("detected_objects", out var objects))
                            {
                                var detectedObjects = JsonSerializer.Deserialize<List<DetectedObject>>(objects.ToString());
                                mainObjects.AddRange(detectedObjects?.Select(o => o.Label) ?? Enumerable.Empty<string>());
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erreur de désérialisation des métadonnées pour l'image {ImageId}", image.Id);
                        }
                    }
                }
            }

            var result = new DocumentDto
            {
                Id = document.Id,
                Name = document.Name,
                Description = document.Description,
                UploadDate = document.UploadDate,
                FileUrl = document.FileUrl,
                Text = document.Text,
                UserId = document.UserId,
                IsAnalyzed = document.IsAnalyzed,
                AnalysisDate = document.AnalysisDate,
                ImagesExtracted = document.ImagesExtracted,
                ImagesExtractedDate = document.ImagesExtractedDate,
                Images = document.Images?.Select(i => new ImageDto
                {
                    Id = i.Id,
                    FileUrl = i.FileUrl,
                    Description = i.Description
                }).ToList() ?? new List<ImageDto>(),
                AnalysisSummary = new AnalysisSummaryDto
                {
                    TotalImages = document.Images?.Count ?? 0,
                    TotalObjectsDetected = document.DetectedObjectsCount ?? 0,
                    MainObjects = mainObjects.Distinct().ToList(), // Élimine les doublons
                    ProcessingTimeSec = document.ProcessingTimeSec ?? 0,
                    AnalysisModel = document.AnalysisModel
                }
            };

            return Ok(result);
        }
        // [PUT] api/documents/{id} - CONSERVE L'EXISTANT

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(
           Guid id,
           [FromBody] DocumentUpdateDto updateDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            var document = await _documentRepository.GetDocumentByIdAsync(id, userId, true); // includeTracking = true
            if (document == null)
            {
                return NotFound("Document non trouvé.");
            }

            // Mise à jour des champs
            if (!string.IsNullOrEmpty(updateDto.Name))
            {
                document.Name = updateDto.Name;
            }

            if (updateDto.Description != null)
            {
                document.Description = updateDto.Description;
            }

            try
            {
                await _documentRepository.SaveChangesAsync();
                return Ok(new
                {
                    Id = document.Id,
                    Name = document.Name,
                    Description = document.Description
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du document {DocumentId}", id);
                return StatusCode(500, "Erreur lors de la mise à jour");
            }
        }

        // [DELETE] api/documents/{id} - CONSERVE L'EXISTANT
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            try
            {
                var result = await _documentRepository.DeleteDocumentAsync(id, userId);
                if (result == null)
                {
                    return NotFound("Document non trouvé.");
                }

                return Ok(new
                {
                    Message = "Document supprimé avec succès",
                    DocumentId = id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du document {DocumentId}", id);
                return StatusCode(500, new
                {
                    Error = "Erreur lors de la suppression",
                    Message = ex.Message
                });
            }
        }

        // [POST] api/documents/{id}/analyze - NOUVEL ENDPOINT
        [HttpPost("{id}/analyze")]
        public async Task<IActionResult> AnalyzeDocument(Guid id)
        {
            try
            {
                // 1. Authentification et validation
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }

                // 2. Récupération du document
                var document = await _documentRepository.GetDocumentByIdAsync(id, userId, includeTracking: true);
                if (document == null)
                {
                    return NotFound("Document non trouvé.");
                }

                // 3. Vérification des prérequis
                if (!document.ImagesExtracted || string.IsNullOrEmpty(document.Text))
                {
                    return BadRequest(new
                    {
                        Message = "L'analyse nécessite une extraction préalable des images et du texte",
                        ImagesExtracted = document.ImagesExtracted,
                        TextExtracted = !string.IsNullOrEmpty(document.Text)
                    });
                }

                if (document.IsAnalyzed)
                {
                    return Conflict(new
                    {
                        Message = "Document déjà analysé",
                        AnalysisDate = document.AnalysisDate
                    });
                }

                // 4. Préparation de la requête pour le service Python
                var analysisRequest = new
                {
                    document_id = id.ToString(),
                    extracted_text = document.Text,
                    images = document.Images?.Select(i => new
                    {
                        id = i.Id.ToString(),
                        image_url = i.FileUrl,
                        description = i.Description
                    }).ToList()
                };

                // 5. Appel au service Python
                var httpClient = _httpClientFactory.CreateClient("PythonService");
                var response = await httpClient.PostAsJsonAsync("analyze", analysisRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erreur du service Python: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, new
                    {
                        Error = "Erreur du service d'analyse",
                        Details = errorContent
                    });
                }

                // 6. Traitement de la réponse
                var result = await response.Content.ReadFromJsonAsync<PythonAnalysisResponse>();
                if (result == null || result.Status != "success")
                {
                    return BadRequest("Réponse d'analyse invalide");
                }

                // 7. Mise à jour des images
                foreach (var imgAnalysis in result.Images ?? Enumerable.Empty<PythonImageAnalysis>())
                {
                    var image = document.Images?.FirstOrDefault(i => i.Id.ToString() == imgAnalysis.ImageId);
                    if (image != null)
                    {
                        image.AnnotatedImageUrl = imgAnalysis.AnnotatedImage;
                        image.Description = imgAnalysis.LlavaDescription;
                        image.Metadata = JsonSerializer.Serialize(new
                        {
                            detected_objects = imgAnalysis.DetectedObjects,
                            analysis_model = "YOLOv8n+LLaVA"
                        });
                    }
                }

                // Mise à jour du document avec les stats réelles
                document.IsAnalyzed = true;
                document.AnalysisDate = DateTime.UtcNow;
                document.DetectedObjectsCount = result.Images?.Sum(i => i.DetectedObjects?.Count ?? 0) ?? 0;
                document.ProcessingTimeSec = result.Stats?.ProcessingTimeSec ?? 0; // Utilisation de la valeur réelle
                document.AnalysisModel = "YOLOv8n+LLaVA";

                await _documentRepository.SaveChangesAsync();

                // Calcul des mainObjects
                var mainObjects = result.Images?
                    .SelectMany(i => i.DetectedObjects?.Select(o => o.Label) ?? Enumerable.Empty<string>())
                    .Distinct()
                    .ToList();

                return Ok(new
                {
                    status = result.Status,
                    images = result.Images?.Select(i => new {
                        image_id = i.ImageId,
                        original_image = i.OriginalImage,
                        annotated_image = i.AnnotatedImage,
                        detected_objects = i.DetectedObjects,
                        llava_description = i.LlavaDescription
                    }),
                    stats = new
                    {
                        pageCount = result.Stats?.PageCount ?? 0,
                        processingTimeSec = result.Stats?.ProcessingTimeSec ?? 0,
                        imagesAnalyzed = result.Images?.Count ?? 0 // Valeur réelle
                    },
                    documentId = id,
                    analysisDate = document.AnalysisDate,
                    analysisSummary = new
                    {
                        totalImages = document.Images?.Count ?? 0,
                        totalObjectsDetected = document.DetectedObjectsCount ?? 0,
                        mainObjects = mainObjects,
                        processingTimeSec = document.ProcessingTimeSec,
                        analysisModel = document.AnalysisModel
                    }
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Erreur de désérialisation JSON");
                return StatusCode(500, new
                {
                    error = "Erreur de format de données",
                    message = jsonEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'analyse du document {DocumentId}", id);
                return StatusCode(500, new
                {
                    error = "Erreur interne",
                    message = ex.Message
                });
            }
        }
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
        [HttpPost("{id}/extract-text")]
        public async Task<IActionResult> ExtractText(Guid id)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }

                // Vérification que le document existe et appartient à l'utilisateur
                var document = await _documentRepository.GetDocumentByIdAsync(id, userId);
                if (document == null)
                {
                    return NotFound("Document non trouvé.");
                }
                // Extraction du texte
                var extractedText = await _documentRepository.ExtractTextFromPdfAsync(id);

                // Mise à jour du document avec le texte extrait (optionnel)
                document.Text = extractedText;
                await _documentRepository.UpdateDocumentAsync(id, document, userId);

                return Ok(new
                {
                    DocumentId = id,
                    ExtractedText = extractedText,
                    Message = "Texte extrait avec succès"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction du texte");
                return StatusCode(500, new
                {
                    Error = "Erreur lors de l'extraction",
                    Message = ex.Message
                });
            }
        }


        [HttpPatch("{id}/update-description")]
        public async Task<IActionResult> UpdateDescriptionFromAnalysis(
            Guid id,
            [FromBody] UpdateDescriptionDto descriptionDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            var document = await _documentRepository.GetDocumentByIdAsync(id, userId, true); // includeTracking = true
            if (document == null)
            {
                return NotFound("Document non trouvé.");
            }

            document.Description = descriptionDto.NewDescription;
            await _documentRepository.SaveChangesAsync();

            return NoContent();
        }


    }


    // Classes pour la réponse d'analyse (à mettre dans un fichier séparé si nécessaire)
    public class AnalysisResult
    {
        [JsonPropertyName("text_content")]
        public string TextContent { get; set; }

        public List<ImageAnalysis> Images { get; set; }
        public AnalysisStats Stats { get; set; }
    }

    public class ImageAnalysis
    {
        [JsonPropertyName("original_image")]
        public string OriginalImage { get; set; }

        [JsonPropertyName("annotated_image")]
        public string AnnotatedImage { get; set; }

        [JsonPropertyName("detected_objects")]
        public List<DetectedObject> DetectedObjects { get; set; }

        [JsonPropertyName("llava_description")]
        public string LlavaDescription { get; set; }
    }

    public class DetectedObject
    {
        public string Label { get; set; }
        public float Confidence { get; set; }
        public float[] Bbox { get; set; }
        public int Area { get; set; }
    }

    public class AnalysisStats
    {
        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }

        [JsonPropertyName("processing_time_sec")]
        public double ProcessingTimeSec { get; set; }

        [JsonPropertyName("images_analyzed")]
        public int ImagesAnalyzed { get; set; }
    }
    // Ajoutez cette classe pour matcher exactement la réponse Python
    public class PythonAnalysisResponse
    {
        public string Status { get; set; }
        public string TextContent { get; set; }
        public List<PythonImageAnalysis> Images { get; set; }
        public PythonAnalysisStats Stats { get; set; }
    }

    public class PythonImageAnalysis
    {
        [JsonPropertyName("image_id")]
        public string ImageId { get; set; }  // Ajout de cette propriété

        [JsonPropertyName("original_image")]
        public string OriginalImage { get; set; }

        [JsonPropertyName("annotated_image")]
        public string AnnotatedImage { get; set; }

        [JsonPropertyName("detected_objects")]
        public List<DetectedObject> DetectedObjects { get; set; }

        [JsonPropertyName("llava_description")]
        public string LlavaDescription { get; set; }

        [JsonPropertyName("detailed_description")]
        public object DetailedDescription { get; set; }
    }

    public class PythonAnalysisStats
    {
        public int PageCount { get; set; }
        public double ProcessingTimeSec { get; set; }
        public int ImagesAnalyzed { get; set; }
    }
}