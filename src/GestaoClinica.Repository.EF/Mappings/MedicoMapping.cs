using GestaoClinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoClinica.Repository.EF.Mappings;

public class MedicoMapping : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nome).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Crm).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Especialidade).IsRequired().HasMaxLength(100);

        // Clínica 1—N Médico: ao excluir a clínica, os médicos dela vão junto.
        builder.HasOne(m => m.Clinica)
            .WithMany(c => c.Medicos)
            .HasForeignKey(m => m.ClinicaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
