using Domain.Models.Xml;
using Microsoft.EntityFrameworkCore;

namespace Adapter.OutBound.SqlAdapter.Context
{
    public class SqlContext : DbContext
    {
        public SqlContext(DbContextOptions<SqlContext> options)
            : base(options)
        {
        }

        public DbSet<Xml> Xmls => Set<Xml>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<Xml>();

            // tabela existente no banco.
            entity.ToTable("t000xml");

            // mapeamento utilizado apenas para consulta.
            entity.HasNoKey();

            // colunas da tabela.
            entity.Property(x => x.TipoMovimento)
                .HasColumnName("tipoMovimento");

            entity.Property(x => x.TipoNI)
                .HasColumnName("tipoNI");

            entity.Property(x => x.NI)
                .HasColumnName("NI");

            entity.Property(x => x.AnoMesCaixa)
                .HasColumnName("anoMesCaixa");

            entity.Property(x => x.NumeroRecibo)
                .HasColumnName("numeroRecibo");

            entity.Property(x => x.Id)
                .HasColumnName("id");
        }
    }
}