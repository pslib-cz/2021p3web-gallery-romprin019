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

        public IList<Album> Album { get;set; }

        public async Task OnGetAsync()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            Album = await _context.Albums
                .Include(a => a.Creator)
                .Where(a => a.CreatorId == userId)
                .ToListAsync();
        }
    }
}
