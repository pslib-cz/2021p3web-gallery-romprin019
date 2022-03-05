#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using galerie_projekt.Data;
using galerie_projekt.Model;
using Microsoft.AspNetCore.Authorization;

namespace galerie_projekt.Pages
{
    [Authorize]
    public class AlbumEditModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;
        public string nameofalbum { get; set; }

        public AlbumEditModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        public IList<AlbumImage> AlbumImages { get; set; }
        public IList<StoredImage> StoredImages { get; set; }

        [BindProperty]
        public Album Album { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            AlbumImages = await _context.AlbumImages
                .Include(a => a.Album)
                .Include(a => a.StoredImage)
                .Where(a => a.AlbumId == id)
                .ToListAsync();
            if (id == null)
            {
                return NotFound();
            }

            Album = await _context.Albums.FirstOrDefaultAsync(m => m.Id == id);
            
            if (Album == null)
            {
                return NotFound();
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var a = await _context.Albums.FirstOrDefaultAsync(x => x.Id == Album.Id);
            a.IsPublic = Album.IsPublic;
            a.Name = Album.Name;
            Album = a;
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
            //if(a.IsPublic == true)
            //{
            //    foreach(var album in AlbumImages.Where(p => p.AlbumId == a.Id))
            //    {
            //        album.StoredImage.IsPublic == true;
            //    }
            //}
            _context.Attach(Album).State = EntityState.Modified;
            /*if(Album.IsPublic == true)
            {
                if(Album.ImagesInAlbum.Count > 0)
                {

                }
            }*/

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlbumExists(Album.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./AlbumList");
        }

        private bool AlbumExists(Guid id)
        {
            return _context.Albums.Any(e => e.Id == id);
        }
    }
}
