using library.Models;
using library.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace library
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(                          //admin123
                new User { Id = 1, UserName = "Admin", HashedPassword = "$2a$11$9hFA9jbWSimBZqpBLBxa4.2kieITIm94n6ckaNCoTzpImOn3hvEdC",  Role = Role.Admin , CreatedAt = new DateTime(2026,7,22) }
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
