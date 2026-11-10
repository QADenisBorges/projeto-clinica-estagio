using GestaoClinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoClinica.Repository.EF.Mappings;

// O mapeamento fica aqui, e não na entidade, para o Domain não conhecer o Entity Framework.
public class ClinicaMapping : IEntityTypeConfiguration<Clinica>
{
    public void Configure(EntityTypeBuilder<Clinica> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Cnpj).IsRequired().HasMaxLength(14);
        builder.Property(c => c.RazaoSocial).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NomeFantasia).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Telefone).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(200);

        // Num banco de verdade este índice impede CNPJ repetido.
        // O banco em memória ignora o índice, por isso o ClinicaService também valida.
        builder.HasIndex(c => c.Cnpj).IsUnique();
    }
}
