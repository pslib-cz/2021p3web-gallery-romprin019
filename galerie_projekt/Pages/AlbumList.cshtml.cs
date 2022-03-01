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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace galerie_projekt.Pages
{
    [Authorize]
    public class AlbumListModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;
        

        public AlbumListModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
            
        }

        public IList<Album> Album { get;set; }
        

        public async Task OnGetAsync()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            Album = await _context.Albums
                .Include(a => a.Creator)
                .Where(a => a.CreatorId == userId)
                .ToListAsync();
        }
        public async Task<IActionResult> OnGetThumbnail(Guid filename, ThumbnailType type = ThumbnailType.Square)
        {
            AlbumImage file = await _context.AlbumImages
              .AsNoTracking()
              .Where(p => p.AlbumId == filename)
              .SingleOrDefaultAsync();
              
            if (file == null)
            {
                return NotFound("no record for this file");
            }
            Thumbnail thumbnail = await _context.Thumbnails
              .AsNoTracking()
              .Where(t => t.FileId == filename && t.Type == type)
              .SingleOrDefaultAsync();
            if (thumbnail != null)
            {
                return File(thumbnail.Blob, file.StoredImage.ContentType);
            }
            return NotFound("no thumbnail for this file");
        }
    }
}
