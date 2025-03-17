using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Docvision.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string RefreshToken { get; set; }
    }

}
