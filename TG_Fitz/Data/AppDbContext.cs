using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TG_Fitz.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=mytgdb.csxisym80s1w.us-east-1.rds.amazonaws.com;" +
                    "Database=MyTGBotFitz;" +
                    "User Id=admin;" +
                    "Password=Armada732820777Qzwerty+;" +
                    "TrustServerCertificate=True;",
                    options => options.EnableRetryOnFailure());
            }
        }
    }
}
