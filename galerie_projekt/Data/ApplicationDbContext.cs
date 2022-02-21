using galerie_projekt.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace galerie_projekt.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Album> Albums { get; set; }
        public DbSet<StoredImage> Images { get; set; }
        public DbSet<Thumbnail> Thumbnails { get; set; }
        public DbSet<AlbumImage> AlbumImages { get; set;}

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<Thumbnail>().HasKey(t => new { t.FileId, t.Type });
            mb.Entity<AlbumImage>().HasKey(t => new { t.FileId, t.AlbumId });
            mb.Entity<AlbumImage>()
            .HasOne(ma => ma.Album)
            .WithMany(m => m.ImagesInAlbum)
            .HasForeignKey(ma => ma.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);
            
            
            
            mb.Entity<AlbumImage>()
               .HasOne(ma => ma.StoredImage)
               .WithMany(a => a.ImagesInAlbum)
               .HasForeignKey(ma => ma.FileId)
               .OnDelete(DeleteBehavior.Restrict);


        }

    }
}