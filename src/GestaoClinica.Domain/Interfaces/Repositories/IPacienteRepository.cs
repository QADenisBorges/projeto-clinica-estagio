using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Interfaces.Base;

namespace GestaoClinica.Domain.Interfaces.Repositories;

public interface IPacienteRepository : IRepositoryBase<Paciente>
{
    Task<Paciente?> BuscarPorCpfAsync(string Cpf);
}
