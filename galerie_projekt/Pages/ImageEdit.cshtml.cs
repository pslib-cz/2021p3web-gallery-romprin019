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

namespace galerie_projekt.Pages
{
    public class ImageEditModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;

        public ImageEditModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StoredImage StoredImage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            StoredImage = await _context.Images
                .Include(s => s.Uploader).FirstOrDefaultAsync(m => m.Id == id);

            if (StoredImage == null)
            {
                return NotFound();
            }
           ViewData["UploaderId"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var a = await _context.Images.FirstOrDefaultAsync(x => x.Id == StoredImage.Id);
            a.IsPublic = StoredImage.IsPublic;
            StoredImage = a;
            _context.Attach(StoredImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoredImageExists(StoredImage.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./AllUserImages");
        }

        private bool StoredImageExists(Guid id)
        {
            return _context.Images.Any(e => e.Id == id);
        }
    }
}
