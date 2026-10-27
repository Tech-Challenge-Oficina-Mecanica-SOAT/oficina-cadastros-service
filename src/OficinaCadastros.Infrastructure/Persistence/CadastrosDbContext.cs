using Microsoft.EntityFrameworkCore;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Persistence;

public class CadastrosDbContext : DbContext, IUnitOfWork
{
    public CadastrosDbContext(DbContextOptions<CadastrosDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Cliente>(e =>
        {
            e.ToTable("clientes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.Cpf).HasMaxLength(11).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(30);
            e.HasIndex(x => x.Cpf).IsUnique();
        });

        b.Entity<Veiculo>(e =>
        {
            e.ToTable("veiculos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Placa).HasMaxLength(10).IsRequired();
            e.Property(x => x.Modelo).HasMaxLength(100).IsRequired();
            e.Property(x => x.Marca).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Placa).IsUnique();
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId);
        });

        b.Entity<Peca>(e =>
        {
            e.ToTable("pecas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.PrecoUnitario).HasPrecision(12, 2);
            e.HasIndex(x => x.Codigo).IsUnique();
        });

        b.Entity<Servico>(e =>
        {
            e.ToTable("servicos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.PrecoUnitario).HasPrecision(12, 2);
            e.Property(x => x.TempoEstimadoHoras).HasPrecision(6, 2);
        });

        b.Entity<OutboxEvent>(e =>
        {
            e.ToTable("outbox_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(100).IsRequired();
            e.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            e.Property(x => x.LastError).HasMaxLength(1000);
            // índice parcial: só os pendentes, que são os que o publicador consulta
            e.HasIndex(x => x.OccurredAt).HasFilter("\"PublishedAt\" IS NULL");
        });
    }
}
