using galerie_projekt.Data;
using galerie_projekt.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace galerie_projekt.Pages
{
    [Authorize]
    public class AllUserImagesModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ILogger<AllUserImagesModel> _logger;
        private ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public List<ImageListViewModel> Images { get; set; } = new List<ImageListViewModel>();

        public AllUserImagesModel(ILogger<AllUserImagesModel> logger, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            Images = _context.Images
            .AsNoTracking()
            .Include(f => f.Uploader)
            .Include(f => f.Thumbnails)
            .Where(f => f.UploaderId == userId)
            .OrderByDescending(f => f.UploadedAt)
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


        }

        public IActionResult OnGetDownload(string filename)
        {
            var fullName = Path.Combine(_environment.ContentRootPath, "Uploads", filename);
            if (System.IO.File.Exists(fullName)) // existuje soubor na disku?
            {
                var fileRecord = _context.Images.Find(Guid.Parse(filename));
                if (fileRecord != null) // je soubor v datab·zi?
                {
                    return PhysicalFile(fullName, fileRecord.ContentType, fileRecord.OriginalName);
                    // vraù ho zp·tky pod p˘vodnÌm n·zvem a typem
                }
                else
                {
                    ErrorMessage = "There is no record of such file.";
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
        }
    }
}
