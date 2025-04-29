using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Docvision.Models
{
    public class DocumentImage
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public string? Description { get; set; }
        
        public string? Objects { get; set; }
        public Guid DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }
    }
}