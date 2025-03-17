using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Docvision.Models;  // Document et DocumentImage

namespace Docvision.Persistance
{
    public class DocContext : IdentityDbContext<ApplicationUser>
    {
        public DocContext(DbContextOptions<DocContext> options)
            : base(options)
        {
        }

        // Ajout des entités Document et DocumentImage
        public DbSet<DocumentImage> Images { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Appel de la configuration de IdentityDbContext

            modelBuilder.Entity<Document>()
                .HasMany(r => r.Images)
                .WithOne(i => i.Document)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Document>()
       .HasOne(d => d.User)
       .WithMany() // Utilise .WithMany() ou .HasMany() selon la direction de la relation
       .HasForeignKey(d => d.UserId) // Utilise UserId comme clé étrangère
       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
