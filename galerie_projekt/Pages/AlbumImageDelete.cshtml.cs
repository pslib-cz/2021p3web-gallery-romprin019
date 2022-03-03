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
    public class AlbumImageDeleteModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;

        public AlbumImageDeleteModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IList<AlbumImage> AlbumImages { get; set; }
        public IList<StoredImage> StoredImage { get; set; }
        [BindProperty]
        public List<ImageSelectVM> GalleryPictures { get; set; }
        public Guid Albumid { get; set; }

        public class ImageSelectVM
        {
            public Guid FileId { get; set; }
            public Guid AlbumId { get; set; }
            public bool IsChecked { get; set; }
        }

        public async Task OnGetAsync(Guid id)
        {
            Albumid = id;
            

            GalleryPictures = await _context.AlbumImages
                .Include(a => a.StoredImage)
                .Include(a => a.Album)
                .Where(b => b.AlbumId == id)
                .Select(a => new ImageSelectVM
                {
                    AlbumId = a.AlbumId,
                    FileId = a.FileId,
                    IsChecked = false
                })
                .ToListAsync();
            
            
            ;

        }
        public async Task<IActionResult> OnPost(Guid albumid)
        {


            foreach (var item in GalleryPictures)
            {
                if (!item.IsChecked) continue;
                var pic = _context.AlbumImages.FirstOrDefault(p => p.FileId == item.FileId);
                
                if (pic is not null)
                {
                    _context.AlbumImages.Remove(pic);
                }
            }
            _context.SaveChanges();
            return RedirectToPage("./AlbumList");
        }
    }
}
