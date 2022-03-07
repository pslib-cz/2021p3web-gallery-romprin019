using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace galerie_projekt.Model
{
    public class StoredImage
    {
        [Key]
        public Guid Id { get; set; } // identifikátor souboru a název fyzického souboru
        [ForeignKey("UploaderId")]
        public AppUser Uploader { get; set; } // kdo soubor nahrál
        [Required]
        public string UploaderId { get; set; } // identifikátor uživatele, který soubor nahrál
        [Required]
        public DateTime UploadedAt { get; set; } // datum a čas nahrání souboru
        [Required]
        public string OriginalName { get; set; } // původní název souboru
        [Required]
        public string ContentType { get; set; } // druh obsahu v souboru (MIME type)
        public bool IsPublic { get; set; }
        public string? TakenAt { get; set; }
        public ICollection<Thumbnail> Thumbnails { get; set; } // kolekce všech možných náhledů
        public ICollection<AlbumImage> ImagesInAlbum { get; set; }
    }
}
