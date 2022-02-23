using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace galerie_projekt.Model
{
    public class Album
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        
        [ForeignKey("CreatorId")]
        public AppUser Creator { get; set; }
        [Required]
        public string CreatorId { get; set; }

        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<AlbumImage> ImagesInAlbum { get; set; }

    }
}
