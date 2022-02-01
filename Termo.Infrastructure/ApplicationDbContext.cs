using Microsoft.EntityFrameworkCore;
using Termo.Models.Entities;

namespace Termo.Infrastructure
{
    public class ApplicationDbContext : DbContext {

        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public virtual DbSet<PlayerEntity> Players { get; set; }
        public virtual DbSet<WorldEntity> Worlds { get; set; }
        public virtual DbSet<TryEntity> Tries { get; set; }
        public virtual DbSet<InvalidWorldEntity> InvalidWorlds { get; set; }

    }
}
