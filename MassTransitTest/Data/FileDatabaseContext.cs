using MassTransit;
using MassTransitTest.Sagas;
using Microsoft.EntityFrameworkCore;

namespace MassTransitTest.Data
{
    public class FileDatabaseContext : DbContext
    {
        public DbSet<FileData> Files { get; set; }
        public DbSet<FileState> FileSaga { get; set; }

        public FileDatabaseContext(DbContextOptions<FileDatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<FileState>().HasKey(x => x.CorrelationId)
                .HasName("FileSagaData");
        }
    }
}
