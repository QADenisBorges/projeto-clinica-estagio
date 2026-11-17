using GestaoClinica.Domain.Entities;

namespace GestaoMedico.Domain.Interfaces;

// Contrato do service de clínica: diz O QUE ele faz. O MedicoService diz COMO.
// O controller depende desta interface, e não da classe, igual ao que o service
// faz com o IMedicoRepository.
public interface IMedicoService
{
    Task<List<Medico>> ListarAsync();
    Task<Medico> BuscarPorIdAsync(int id);
    Task<Medico> CriarAsync(Medico Medico);
    Task<Medico> AtualizarAsync(int id, Medico dados);
    Task ExcluirAsync(int id);
}
