#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using galerie_projekt.Data;
using galerie_projekt.Model;

namespace galerie_projekt.Pages
{
    public class AlbumImagesModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;

        public AlbumImagesModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<AlbumImage> AlbumImage { get;set; }
        public string albumname { get; set; }
        
        public async Task OnGetAsync(Guid id)
        {
            
            AlbumImage = await _context.AlbumImages
                .Include(a => a.Album)
                .Include(a => a.StoredImage)
                .Where(a => a.AlbumId == id)
                .ToListAsync();

            var album = _context.Albums.Where(p => p.Id == id).FirstOrDefault();
            albumname = album.Name;
                        
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
        public async Task<IActionResult> OnGetDeleteAsync(Guid id)
        {
            

            var item = _context.AlbumImages
                .Where(p => p.FileId == id).FirstOrDefault();
            if (item != null)
            {            
                _context.AlbumImages.Remove(item);
            }
            _context.SaveChanges();
            return Page();

        }
        public async Task<IActionResult> OnGetDeleteAllAsync(Guid id)
        {
            
            var item = _context.Images
                .Where(p => p.Id == id).SingleOrDefault();
            var item2 = _context.AlbumImages
                .Where(p => p.FileId == id).ToList();
            if (item != null)
            {
                _context.Images.Remove(item);
                _context.AlbumImages.RemoveRange(item2);
            }
            _context.SaveChanges();
            await OnGetAsync(id);
            return Page();

        }
    }
}
