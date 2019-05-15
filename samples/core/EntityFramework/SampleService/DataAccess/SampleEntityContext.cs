using DataArt.Atlas.EntityFramework.MsSql.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace SampleService.DataAccess
{
    public sealed class SampleEntityContext : BaseDbContext<SampleEntityContext>
    {
        // for Add-Migration
        public SampleEntityContext() 
            : base(new DbContextOptionsBuilder<SampleEntityContext>().UseSqlServer("Server=localhost;Database=Atlas;Integrated Security=True;").Options)
        {
        }

        public SampleEntityContext(DbContextOptions<SampleEntityContext> options)
            : base(options)
        {
        }

        public DbSet<SampleEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(Application.SampleService.Key);

            modelBuilder.Entity<SampleEntity>();
        }
    }
}
