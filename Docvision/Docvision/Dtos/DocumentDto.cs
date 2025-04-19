using Back.Controllers;
using System.ComponentModel.DataAnnotations;

namespace Docvision.Dtos
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; } // Champ unique pour la description
        public bool IsAnalyzed { get; set; }
        public DateTime? AnalysisDate { get; set; }
        public AnalysisSummaryDto? AnalysisSummary { get; set; }

        public DateTime UploadDate { get; set; }
        public string FileUrl { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public bool ImagesExtracted { get; set; }
        public DateTime? ImagesExtractedDate { get; set; }
        public List<ImageDto> Images { get; set; }
    }

    public class ImageDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public string Description { get; set; }
    }
    public class ImageExtractionResultDto
    {
        public string Message { get; set; }
        public int Count { get; set; }
        public List<ImageDto> Images { get; set; }
    }
    public class DocumentUpdateDto
    {
        [StringLength(255, ErrorMessage = "Le nom ne peut dépasser 255 caractères")]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
    public class UpdateDescriptionDto
    {
        [Required]
        public string NewDescription { get; set; }
    }
    public class AnalysisSummaryDto
    {
        public int TotalImages { get; set; }
        public int TotalObjectsDetected { get; set; }
        public List<string>? MainObjects { get; set; }
        public double? ProcessingTimeSec { get; set; }
        public string? AnalysisModel { get; set; }
    }
    public class ImageAnalysisDto
    {
        public List<DetectedObject>? DetectedObjects { get; set; }
        public string? AnalysisModel { get; set; }
    }

    public class DocumentAnalysisResponseDto
    {
        public Guid DocumentId { get; set; }
        public string Status { get; set; }
        public DateTime AnalysisDate { get; set; }
        public AnalysisSummaryDto AnalysisSummary { get; set; }
    }


}
