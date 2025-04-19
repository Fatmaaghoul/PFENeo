using System.ComponentModel.DataAnnotations.Schema;

namespace Docvision.Models
{
    public class DocumentImage
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public string? Description { get; set; }
        public string? AnnotatedImageUrl { get; set; }  // URL de l'image annotée
        public string? Metadata { get; set; }          // Stockage JSON des détections
        public Guid DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }
    }
}
