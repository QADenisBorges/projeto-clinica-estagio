using GestaoClinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestaoClinica.Repository.EF.Context;

// O DbContext é a "porta" do Entity Framework para o banco. Cada DbSet vira uma tabela.
public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options)
    {
    }

    public DbSet<Clinica> Clinicas => Set<Clinica>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes da pasta Mappings de uma vez.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicaDbContext).Assembly);
    }
}
