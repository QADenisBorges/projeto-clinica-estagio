using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Exceptions;
using GestaoClinica.Domain.Interfaces;
using GestaoMedico.Domain.Interfaces;


namespace GestaoClinica.Domain.Services;

// Todas as regras de negócio de clínica moram aqui.
// Quando uma regra é quebrada, lançamos NegocioException e a API cuida da resposta.
public class MedicoService : IMedicoService
{
    private readonly IMedicoRepository _repositorio;

    public MedicoService(IMedicoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Medico>> ListarAsync()
    {
        return _repositorio.ListarAsync();
    }

    public async Task<Medico> BuscarPorIdAsync(int id)
    {
        var medico = await _repositorio.BuscarPorIdAsync(id);

        if (medico is null)
            throw new RegistroNaoEncontradoException("Medico não encontrada.");

        return medico;
    }

    public async Task<Medico> CriarAsync(Medico clinica)
    {
        await ValidarAsync(clinica);

        await _repositorio.AdicionarAsync(clinica);
        return clinica;
    }

    public async Task<Medico> AtualizarAsync(int id, Medico dados)
    {
        var clinica = await BuscarPorIdAsync(id);

        // Validamos antes de copiar para não alterar a clínica salva com dados inválidos.
        dados.Id = id;
        await ValidarAsync(dados);

        clinica.Nome = dados.Nome;
        clinica.Crm = dados.Crm;
        clinica.Especialidade = dados.Especialidade;

        await _repositorio.AtualizarAsync(clinica);

        return clinica;
    }

    public async Task ExcluirAsync(int id)
    {
        var clinica = await BuscarPorIdAsync(id);
        await _repositorio.RemoverAsync(clinica);
    }

    private async Task ValidarAsync(Medico medico)
    {
        if (string.IsNullOrWhiteSpace(medico.Nome))
            throw new NegocioException("Nome é obrigatória.");

        if (string.IsNullOrWhiteSpace(medico.Crm))
            throw new NegocioException("O Crm é obrigatório.");

        if (string.IsNullOrWhiteSpace(medico.Especialidade))
            throw new NegocioException("A Especialidade é obrigatório.");

        // Nem todo banco garante CNPJ único (o em memória não garante), então a regra é conferida aqui.
        var clinicaComMesmoCnpj = await _repositorio.BuscarPorCrmAsync(medico.Crm);

        if (clinicaComMesmoCnpj is not null && clinicaComMesmoCnpj.Id != medico.Id)
            throw new NegocioException("Já existe uma Medico cadastrada com este Crm.");
    }
}
