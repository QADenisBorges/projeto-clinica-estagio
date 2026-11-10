using GestaoClinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoClinica.Repository.EF.Mappings;

public class PacienteMapping : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Cpf).IsRequired().HasMaxLength(11);

        // Clínica 1—N Paciente: ao excluir a clínica, os pacientes dela vão junto.
        builder.HasOne(p => p.Clinica)
            .WithMany(c => c.Pacientes)
            .HasForeignKey(p => p.ClinicaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
