namespace galerie_projekt.Model
{
    public class AlbumImage
    {
        public Guid FileId { get; set; }
        public StoredImage StoredImage { get; set; }
        public Guid AlbumId { get; set; }
        public Album Album { get; set; }
        public string Description { get; set; }

    }
}
