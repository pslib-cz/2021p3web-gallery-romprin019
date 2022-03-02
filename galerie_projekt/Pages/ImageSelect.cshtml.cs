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
    public class ImageSelectModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;

        public ImageSelectModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public IList<AlbumImage> AlbumImages { get;set; }
        public IList<StoredImage> StoredImage { get;set; }
        

        public async Task OnGetAsync(Guid id)
        {
            AlbumImages = await _context.AlbumImages
                .Where(p => p.AlbumId != id)
                .Include(a => a.StoredImage).ToListAsync();
        }
        public async Task<IActionResult> OnPost(IList<AlbumImage> albumimages)
        {

            var a = albumimages;//await _context.AlbumImages.Where(p => p.IsChecked == true).ToListAsync();
            foreach (var image in a)
            {
                var newimage = new AlbumImage
                {
                    FileId = image.FileId,
                    Description = "Added at" + DateTime.Now.ToString(),
                    AlbumId = image.AlbumId,

                };
                _context.AlbumImages.Add(newimage);
                
            }
            _context.SaveChanges();
            return RedirectToPage("./AlbumList");
        }
        
    }
}
