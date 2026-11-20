using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Domain.Interfaces.Repositories;
using GestaoClinica.Repository.EF.Context;
using GestaoClinica.Repository.EF.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace GestaoClinica.Repository.EF.Repositories;

// Repositório de clínica. Listar, buscar por Id, adicionar e atualizar vêm de RepositoryBase.
// Aqui fica só o que é específico de clínica. Nenhuma regra de negócio: só acesso a dados.
public class PacienteRepository : RepositoryBase<Paciente>, IPacienteRepository
{
    public PacienteRepository(ClinicaDbContext contexto) : base(contexto)
    {
    }

    public Task<Paciente?> BuscarPorCpfAsync(string Cpf)
    {
        return Tabela.FirstOrDefaultAsync(c => c.Cpf == Cpf);
    }
}
