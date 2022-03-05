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
using System.Security.Claims;

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
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;

            Albumid = id;
            var imagesinalbum = await _context.AlbumImages
                .Include(a => a.StoredImage)
                .Include(a => a.Album)
                .Where(p => p.AlbumId == id)
                .ToListAsync(); //obrazky z alaba

            var allimages = await _context.AlbumImages
                .Include(a => a.StoredImage)
                .Include(a => a.Album)
                .Where(p => p.AlbumId == Guid.Parse(userId))
                .ToListAsync(); //obrazky z defaultu


            GalleryPictures = allimages
                .Where(x => imagesinalbum.All(y => y.FileId != x.FileId)).Select(a => new ImageSelectVM
                {
                    AlbumId = a.AlbumId,
                    FileId = a.FileId,
                    IsChecked = false
                })

            .ToList();

        }
        public async Task<IActionResult> OnPost(Guid albumid)
        {

            var currentalbum = _context.Albums.Where(p => p.Id == albumid).FirstOrDefault();
            foreach (var item in GalleryPictures)
            {


                if (!item.IsChecked) continue;
                var pic = _context.AlbumImages
                    .Include(p => p.StoredImage)
                    .FirstOrDefault(p => p.FileId == item.FileId);
                if (currentalbum.IsPublic == true)
                {
                    pic.StoredImage.IsPublic = true;
                }
                if(currentalbum.IsPublic == false)
                {
                    pic.StoredImage.IsPublic = false;
                }
                var albumimage = new AlbumImage
                {
                    FileId = pic.FileId,
                    AlbumId = albumid,
                    Description = "Added at " + DateTime.Now.ToString()

                };
                if (pic is not null)
                {
                    if (albumimage.AlbumId == albumid)
                        _context.AlbumImages.Add(albumimage);
                }
                
            }
            _context.SaveChanges();
            return RedirectToPage("./AlbumList");
        }

    }
}
