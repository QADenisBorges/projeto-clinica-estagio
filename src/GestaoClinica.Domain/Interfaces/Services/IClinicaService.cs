using GestaoClinica.Domain.Entities;

namespace GestaoClinica.Domain.Interfaces;

// Contrato do service de clínica: diz O QUE ele faz. O ClinicaService diz COMO.
// O controller depende desta interface, e não da classe, igual ao que o service
// faz com o IClinicaRepository.
public interface IClinicaService
{
    Task<List<Clinica>> ListarAsync();
    Task<Clinica> BuscarPorIdAsync(int id);
    Task<Clinica> CriarAsync(Clinica clinica);
    Task<Clinica> AtualizarAsync(int id, Clinica dados);
    Task ExcluirAsync(int id);
}
