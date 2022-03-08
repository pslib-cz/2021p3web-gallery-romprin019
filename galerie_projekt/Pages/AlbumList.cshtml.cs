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
using System.ComponentModel.DataAnnotations;

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

        public IList<Album> Album { get; set; }
        public IList<AlbumImage> AlbumImages { get; set; }
        public string creatorid { get; set; }


        public async Task OnGetAsync()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            Album = await _context.Albums
                .Include(a => a.Creator)
                .Where(a => a.CreatorId == userId)
                .ToListAsync();
            creatorid = userId;
        }
        public async Task<IActionResult> OnGetThumbnail(Guid filename, ThumbnailType type = ThumbnailType.Square)
        {
            if (User.Identity.IsAuthenticated)
            {
                foreach (var album in Album)
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
                return Page();
            }
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

        public async Task<IActionResult> OnPostAsync(Guid id, bool IsPublic, string Name)
        {
            var a = await _context.Albums.FirstOrDefaultAsync(x => x.Id == id);
            a.IsPublic = IsPublic;
            a.Name = Name;
            a.ImagesInAlbum = _context.AlbumImages
                .Where(p => p.AlbumId == a.Id)
                .Include(p => p.StoredImage)
                .ToList();
            if (a.IsPublic == true)
            {
                foreach (var image in a.ImagesInAlbum.Where(p => p.AlbumId == a.Id))
                {
                    image.StoredImage.IsPublic = true;
                }
            }
            if (a.IsPublic == false)
            {
                foreach (var image in a.ImagesInAlbum.Where(p => p.AlbumId == a.Id))
                {
                    image.StoredImage.IsPublic = false;
                }
            }

            _context.Attach(a).State = EntityState.Modified;
            await _context.SaveChangesAsync();


            return RedirectToPage("./AlbumList");
        }

    }
}
