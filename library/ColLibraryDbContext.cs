using library.Models;
using Microsoft.EntityFrameworkCore;

namespace library
{
    public class ColLibraryDbContext : DbContext
    {
        public ColLibraryDbContext(DbContextOptions<ColLibraryDbContext> options) : base(options) 
        {
        }

        DbSet<User> Users { get; set; }
        DbSet<Book> Books { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Borrow> Borrows { get; set; }
    }
}
