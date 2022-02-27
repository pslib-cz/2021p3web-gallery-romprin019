#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using galerie_projekt.Data;
using galerie_projekt.Model;

namespace galerie_projekt.Pages
{
    public class TestAlbumImageModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;

        public TestAlbumImageModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public StoredImage StoredImage { get; set; }

        public IActionResult OnGet(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            ViewData["AlbumId"] = new SelectList(_context.Albums, "Id", "Name");
            StoredImage = _context.Images.FirstOrDefault(m => m.Id == id);
            
            return Page();
        }

        [BindProperty]
        public AlbumImage AlbumImage { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            var a = _context.Images.FirstOrDefault(x => x.Id == StoredImage.Id);

            var newalbumimage = new AlbumImage
            {
                FileId =  a.Id,
                AlbumId = AlbumImage.AlbumId,
                Description = AlbumImage.Description
            };

            _context.AlbumImages.Add(newalbumimage);
            await _context.SaveChangesAsync();

            return RedirectToPage("./AllUserImages");
        }
    }
}
