using Microsoft.EntityFrameworkCore;
 
namespace Musicollage.Models
{
    public class MusicContext : DbContext
    {
        public MusicContext(DbContextOptions<MusicContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Release> Releases { get; set; }
    }
    
}