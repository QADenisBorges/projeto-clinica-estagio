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

        // Clínica N—N Médico: um médico pode estar em várias clínicas.
        // O EF cria sozinho a tabela de ligação "ClinicaMedico", que guarda os pares (clínica, médico).
        // Ao excluir a clínica, só as ligações são apagadas: o médico continua cadastrado.
        builder.HasMany(m => m.Clinicas)
            .WithMany(c => c.Medicos)
            .UsingEntity("ClinicaMedico");
    }
}
