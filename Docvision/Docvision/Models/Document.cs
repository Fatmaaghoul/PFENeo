using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Docvision.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; } // Champ unique modifiable par tous

        public DateTime UploadDate { get; set; }
        public string FileUrl { get; set; } = "";
        public string? Text { get; set; }
        // public ICollection<DocumentImage>? Images { get; set; }
        public List<DocumentImage> Images { get; set; } = new List<DocumentImage>();
        public bool IsAnalyzed { get; set; } // Nouveau champ pour suivre l'état d'analyse
        public DateTime? AnalysisDate { get; set; } // Optionnel: date de l'analyse
                                                    //public Guid? UserId { get; set; } 
        public int? DetectedObjectsCount { get; set; }
        public string? AnalysisModel { get; set; }
        public double? ProcessingTimeSec { get; set; }
        public string UserId { get; set; } // Utiliser string au lieu de Guid pour correspondre à IdentityUser

        [ForeignKey("UserId")]
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        public bool ImagesExtracted { get; set; }
        public DateTime? ImagesExtractedDate { get; set; }
    }
}
