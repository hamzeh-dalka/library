using library.Models;
using Microsoft.EntityFrameworkCore;

namespace library
{
    public class ColLibraryDbContext : DbContext
    {
        public ColLibraryDbContext(DbContextOptions<ColLibraryDbContext> options) : base(options) 
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
    }
}
