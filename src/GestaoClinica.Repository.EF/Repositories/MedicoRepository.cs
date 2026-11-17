using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Repository.EF.Context;
using GestaoClinica.Repository.EF.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace GestaoClinica.Repository.EF.Repositories;

// Repositório de clínica. Listar, buscar por Id, adicionar e atualizar vêm de RepositoryBase.
// Aqui fica só o que é específico de clínica. Nenhuma regra de negócio: só acesso a dados.
public class MedicoRepository : RepositoryBase<Medico>, IMedicoRepository
{
    public MedicoRepository(ClinicaDbContext contexto) : base(contexto)
    {
    }

    public Task<Medico?> BuscarPorCrmAsync(string crm)
    {
        return Tabela.FirstOrDefaultAsync(c => c.Crm == crm);
    }
}
