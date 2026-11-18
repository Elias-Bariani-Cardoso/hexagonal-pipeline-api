using Adapter.OutBound.SqlAdapter.Context;
using Microsoft.EntityFrameworkCore;

namespace Integration.Helpers
{
    public class TransactionTestContext : SqlContext
    {
        public TransactionTestContext(
            DbContextOptions<SqlContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransactionTestRecord>(entity =>
            {
                entity.ToTable(
                    "__TransactionTestRecords",
                    "dbo");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .ValueGeneratedNever();

                entity.Property(x => x.Description)
                    .HasMaxLength(100)
                    .IsRequired();
            });
        }
    }
}