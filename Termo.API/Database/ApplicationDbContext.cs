using Microsoft.EntityFrameworkCore;
using Termo.API.Entities;

namespace Termo.API.Database {
    public class ApplicationDbContext : DbContext {

        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public virtual DbSet<PlayerEntity> Players { get; set; }
        public virtual DbSet<WorldEntity> Worlds { get; set; }
        public virtual DbSet<TryEntity> Tries { get; set; }

    }
}
