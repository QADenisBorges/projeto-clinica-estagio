using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Repository.EF.Context;
using GestaoClinica.Repository.EF.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace GestaoClinica.Repository.EF.Repositories;

// Repositório de clínica. Listar, buscar por Id, adicionar e atualizar vêm de RepositoryBase.
// Aqui fica só o que é específico de clínica. Nenhuma regra de negócio: só acesso a dados.
public class ClinicaRepository : RepositoryBase<Clinica>, IClinicaRepository
{
    public ClinicaRepository(ClinicaDbContext contexto) : base(contexto)
    {
    }

    public Task<Clinica?> BuscarPorCnpjAsync(string cnpj)
    {
        return Tabela.FirstOrDefaultAsync(c => c.Cnpj == cnpj);
    }

    // "override" troca o RemoverAsync da classe base por este.
    public override async Task RemoverAsync(Clinica clinica)
    {
        // O banco em memória não apaga os filhos sozinho. Carregando médicos e pacientes,
        // o EF remove todos junto com a clínica, como um banco de verdade faria (cascade).
        await Contexto.Entry(clinica).Collection(c => c.Medicos).LoadAsync();
        await Contexto.Entry(clinica).Collection(c => c.Pacientes).LoadAsync();

        await base.RemoverAsync(clinica);
    }
}
