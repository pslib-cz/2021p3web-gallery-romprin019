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
using Microsoft.AspNetCore.Authorization;

namespace galerie_projekt.Pages
{
    [Authorize]
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
            var x = await _context.AlbumImages
                .Include(a => a.StoredImage)
                .Include(a => a.Album)
                .Where(p => p.AlbumId == id)
                .ToListAsync();
           
            var y = await _context.AlbumImages
                .Include(a => a.StoredImage)
                .Include(a => a.Album)
                //.Where(a => !x.Where(b => b.AlbumId == id).Any())
                //.Select(a => new ImageSelectVM
                //{
                //    AlbumId = a.AlbumId,
                //    FileId = a.FileId,
                //    IsChecked = false
                //})
                .ToListAsync();
            List<AlbumImage> z = new List<AlbumImage>();
            foreach (var image in y)
            {
                if(!x.Contains(image))
                {
                    z.Add(image);
                }

            }
            GalleryPictures = z.Select(a => new ImageSelectVM
            {
                AlbumId = a.AlbumId,
                FileId = a.FileId,
                IsChecked = false
            })
            .ToList();
            
        }
        public async Task<IActionResult> OnPost(Guid albumid)
        {


            foreach (var item in GalleryPictures)
            {
                if (!item.IsChecked) continue;
                var pic = _context.AlbumImages.FirstOrDefault(p => p.FileId == item.FileId);
                var albumimage = new AlbumImage
                {
                    FileId = pic.FileId,
                    AlbumId = albumid,
                    Description = "Added at " + DateTime.Now.ToString()
                };
                if (pic is not null)
                {
                    _context.AlbumImages.Add(albumimage);
                }
            }
            _context.SaveChanges();
            return RedirectToPage("./AlbumList");
        }
        
    }
}
