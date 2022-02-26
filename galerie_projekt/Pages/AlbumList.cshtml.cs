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
        private readonly UserManager<AppUser> _userManager;

        public AlbumListModel(galerie_projekt.Data.ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Album> Album { get;set; }

        public async Task OnGetAsync()
        {
            var user = _userManager.Users.FirstOrDefault();
            Album = await _context.Albums
                //.Include(a => a.Creator)
                //.Where(a => a.Creator.Id == user.Id)
                .ToListAsync();
        }
    }
}
