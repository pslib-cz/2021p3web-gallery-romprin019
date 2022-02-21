using System.ComponentModel.DataAnnotations;

namespace galerie_projekt.Model
{
    public class Album
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<StoredImage> StoredImages { get; set; }

    }
}
