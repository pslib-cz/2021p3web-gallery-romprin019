namespace galerie_projekt.Model
{
    public class ImageListViewModel
    {
        public Guid Id { get; set; }
        public AppUser Uploader { get; set; }
        public string UploaderId { get; set; }
        public DateTime UploadedAt { get; set; }
        public string OriginalName { get; set; }
        public string ContentType { get; set; }
        public int ThumbnailCount { get; set; }
    }
}
