using galerie_projekt.Data;
using galerie_projekt.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace galerie_projekt.Pages
{
    public class IndexModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ILogger<IndexModel> _logger;
        private ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public List<ImageListViewModel> Images { get; set; } = new List<ImageListViewModel>();
        [BindProperty]
        public bool ImageIsPublic { get; set; }
        public IList<Album> Album { get; set; }
        public IList<AlbumImage> AlbumImages { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _logger = logger;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Images = _context.Images
            .AsNoTracking()
            .Include(f => f.Uploader)
            .Include(f => f.Thumbnails)
            .OrderByDescending(f => f.UploadedAt)
            .Where(f => f.IsPublic == true)
            .Take(12)
            .Select(f => new ImageListViewModel
            {
                Id = f.Id,
                ContentType = f.ContentType,
                OriginalName = f.OriginalName,
                UploaderId = f.UploaderId,
                Uploader = f.Uploader,
                UploadedAt = f.UploadedAt,
                IsPublic = f.IsPublic,
                ThumbnailCount = f.Thumbnails.Count
            })
            .ToList();

            Album = await _context.Albums
                .Include(a => a.Creator)
                .Where(a => a.IsPublic == true)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();


        }

        public IActionResult OnGetDownload(string filename)
        {
            var fullName = Path.Combine(_environment.ContentRootPath, "Uploads", filename);
            if (System.IO.File.Exists(fullName)) // existuje soubor na disku?
            {
                var fileRecord = _context.Images.Find(Guid.Parse(filename));
                if (fileRecord != null) // je soubor v databázi?
                {
                    return PhysicalFile(fullName, fileRecord.ContentType, fileRecord.OriginalName);
                    // vrať ho zpátky pod původním názvem a typem
                }
                else
                {
                    ErrorMessage = "There is no record of such file.";
                    _context.Images.Remove(fileRecord);
                    return RedirectToPage();
                }
            }
            else
            {
                ErrorMessage = "There is no such file.";
                return RedirectToPage();
            }
        }
        public async Task<IActionResult> OnGetThumbnail(string filename, ThumbnailType type = ThumbnailType.Square)
        {
            
            
                StoredImage file = await _context.Images
              .AsNoTracking()
              .Where(f => f.Id == Guid.Parse(filename))
              .SingleOrDefaultAsync();
                if (file == null)
                {
                    return NotFound("no record for this file");
                }
                Thumbnail thumbnail = await _context.Thumbnails
                  .AsNoTracking()
                  .Where(t => t.FileId == Guid.Parse(filename) && t.Type == type)
                  .SingleOrDefaultAsync();
                if (thumbnail != null)
                {
                    return File(thumbnail.Blob, file.ContentType);
                }
                return NotFound("no thumbnail for this file");
            
            return BadRequest();

        }
        public async Task<IActionResult> OnGetDeleteAsync(Guid id)
        {
            if (id != null)
            {
                var album = _context.Albums.Where(a => a.Id == id).FirstOrDefault();
                List<AlbumImage> imagesinalbum = _context.AlbumImages.Where(p => p.AlbumId == id).ToList();
                foreach (var item in imagesinalbum)
                {
                    _context.Remove(item);
                }
                _context.Albums.Remove(album);
                _context.SaveChanges();
                await OnGetAsync();
                return Page();
            }
            return Page();
        }
    }
}