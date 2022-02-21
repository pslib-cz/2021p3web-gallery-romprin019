using Microsoft.AspNetCore.Identity;

namespace galerie_projekt.Model
{
    public class AppUser : IdentityUser
    {
        public ICollection<Album> Albums { get; set; }
        public ICollection<StoredImage> StoredImages { get; set; }
    }
}
