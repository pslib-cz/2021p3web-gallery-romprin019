using System.ComponentModel.DataAnnotations;

namespace galerie_projekt.Model
{
    public class Album
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public AppUser Creator { get; set; }
        public ICollection<AlbumImage> ImagesInAlbum { get; set; }

    }
}
