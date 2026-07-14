using library.Models;
using library.Models.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace library
{
    public class Library3DbContext : DbContext
    {
        public Library3DbContext(DbContextOptions<Library3DbContext> options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(                          //admin123
                new User { Id = 1, UserName = "Admin", HashedPassword = "$2a$11$9hFA9jbWSimBZqpBLBxa4.2kieITIm94n6ckaNCoTzpImOn3hvEdC",  Role = Role.Admin }
            );
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<Librarian> Librarians { get; set; } 
        public DbSet<Student> Students { get; set; }
    }
}
