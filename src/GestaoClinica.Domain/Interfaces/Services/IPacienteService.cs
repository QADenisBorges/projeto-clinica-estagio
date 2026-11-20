using GestaoClinica.Domain.Entities;

namespace GestaoClinica.Domain.Interfaces;

// Contrato do service de clínica: diz O QUE ele faz. O ClinicaService diz COMO.
// O controller depende desta interface, e não da classe, igual ao que o service
// faz com o IClinicaRepository.
public interface IPacienteService
{
    Task<List<Paciente>> ListarAsync();
    Task<Paciente> BuscarPorIdAsync(int id);
    Task<Paciente> CriarAsync(Paciente clinica);
    Task<Paciente> AtualizarAsync(int id, Paciente dados);
    Task ExcluirAsync(int id);
}
