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
using System.Security.Claims;

namespace galerie_projekt.Pages
{
    public class AlbumCreateModel : PageModel
    {
        private readonly galerie_projekt.Data.ApplicationDbContext _context;

        public AlbumCreateModel(galerie_projekt.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Album Album { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            var newalbum = new Album
            {
                Id = new Guid(),
                CreatorId = userId,
                CreatedAt = DateTime.Now,
                IsPublic = Album.IsPublic,
                Name = Album.Name

            };
            //Album.IsPublic = Album.IsPublic;
            //Album.CreatedAt = DateTime.Now;
            //Album.CreatorId = userId;
            //Album.Name = Album.Name;
            //Album
            

            _context.Albums.Add(newalbum);
            await _context.SaveChangesAsync();

            return RedirectToPage("./AlbumList");
        }
    }
}
