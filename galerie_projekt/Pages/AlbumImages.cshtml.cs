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
using Microsoft.AspNetCore.Authorization;

namespace galerie_projekt.Pages
{
    [Authorize]
    public class AlbumImagesModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;
        private IWebHostEnvironment _environment;

        public AlbumImagesModel(galerie_projekt.Data.ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IList<AlbumImage> AlbumImage { get;set; }
        public Album currentalbum { get; set; }
        public Guid AlbumId2 { get; set; }
        public string creatorid { get; set; }
        public async Task OnGetAsync(Guid albumid)
        {
            
            AlbumImage = await _context.AlbumImages
                .Include(a => a.Album)
                .Include(a => a.StoredImage)
                .Where(a => a.AlbumId == albumid)
                .ToListAsync();



            var album = _context.Albums.Where(p => p.Id == albumid).FirstOrDefault();
            currentalbum = album;
            AlbumId2 = albumid;
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            userId = creatorid;

        }

        

        
        public async Task<IActionResult> OnGetThumbnail(string filename, ThumbnailType type = ThumbnailType.Square)
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            creatorid = userId;
            if (User.Identity.IsAuthenticated || currentalbum.CreatorId == userId)
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
            return BadRequest();
            
        }

        //public async Task<IActionResult> OnGetDeleteAsync(Guid pictureid, Guid albumid)
        //{
        //    var item = _context.AlbumImages
        //        .Where(p => p.FileId == pictureid).FirstOrDefault();
        //    if (item != null)
        //    {            
        //        _context.AlbumImages.Remove(item);
        //    }
        //    _context.SaveChanges();
        //    await OnGetAsync(albumid);
        //    return Page();
        //}

        public async Task<IActionResult> OnGetDeleteAllAsync(Guid pictureid, Guid albumid)
        {
            
            var item = _context.Images
                .Where(p => p.Id == pictureid).SingleOrDefault();
            var item2 = _context.AlbumImages
                .Where(p => p.FileId == pictureid).ToList();
            if (item != null)
            {
                _context.Images.Remove(item);
                var file = Path.Combine(_environment.ContentRootPath, "Uploads", item.Id.ToString());
                System.IO.File.Delete(file);
                _context.AlbumImages.RemoveRange(item2);
            }
            
            _context.SaveChanges();
            await OnGetAsync(albumid);
            return Page();
        }
        
        public async Task<IActionResult> OnGetImageAsync(Guid pictureid)
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            
            var image = _context.Images.Where(p => p.Id == pictureid).FirstOrDefault();
            if (image.IsPublic == true || image.UploaderId == userId)
            {
                if (pictureid == null)
                {
                    return NotFound();
                }
                if (pictureid != null)
                {
                    
                    var file = Path.Combine(_environment.ContentRootPath, "Uploads", pictureid.ToString());

                    using (var fs = new FileStream(file, FileMode.Open))
                    {
                        MemoryStream ms = new MemoryStream();
                        fs.CopyTo(ms);
                        return File(ms.ToArray(), image.ContentType);
                    }

                }

                return Page();
            }
            return RedirectToPage("Error");

            
        }
        public async Task<IActionResult> OnGetSort(Guid albumid, string optionnum)
        {
            switch (optionnum)
            {
                case "1":
                    AlbumImage = await _context.AlbumImages
                .Include(a => a.Album)
                .Include(a => a.StoredImage)
                .Where(a => a.AlbumId == albumid)
                .OrderByDescending(a => a.StoredImage.UploadedAt)
                .ToListAsync();
                    break;

                case "2":
                    AlbumImage = await _context.AlbumImages
                .Include(a => a.Album)
                .Include(a => a.StoredImage)
                .Where(a => a.AlbumId == albumid)
                .OrderBy(a => a.StoredImage.UploadedAt)
                .ToListAsync();
                    break;

                case "3":
                    AlbumImage = await _context.AlbumImages
                .Include(a => a.Album)
                .Include(a => a.StoredImage)
                .Where(a => a.AlbumId == albumid)
                .OrderBy(a => a.StoredImage.OriginalName)
                .ToListAsync();
                    break;  
            }
            AlbumId2 = albumid;
            
            

            return Page();
        }
    }
}

