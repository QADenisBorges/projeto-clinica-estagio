using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Exceptions;
using GestaoClinica.Domain.Interfaces;

namespace GestaoClinica.Domain.Services;

// Todas as regras de negócio de clínica moram aqui.
// Quando uma regra é quebrada, lançamos NegocioException e a API cuida da resposta.
public class ClinicaService : IClinicaService
{
    private readonly IClinicaRepository _repositorio;

    public ClinicaService(IClinicaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Clinica>> ListarAsync()
    {
        return _repositorio.ListarAsync();
    }

    public async Task<Clinica> BuscarPorIdAsync(int id)
    {
        var clinica = await _repositorio.BuscarPorIdAsync(id);

        if (clinica is null)
            throw new RegistroNaoEncontradoException("Clínica não encontrada.");

        return clinica;
    }

    public async Task<Clinica> CriarAsync(Clinica clinica)
    {
        await ValidarAsync(clinica);

        await _repositorio.AdicionarAsync(clinica);
        return clinica;
    }

    public async Task<Clinica> AtualizarAsync(int id, Clinica dados)
    {
        var clinica = await BuscarPorIdAsync(id);

        // Validamos antes de copiar para não alterar a clínica salva com dados inválidos.
        dados.Id = id;
        await ValidarAsync(dados);

        clinica.Cnpj = dados.Cnpj;
        clinica.RazaoSocial = dados.RazaoSocial;
        clinica.NomeFantasia = dados.NomeFantasia;
        clinica.Telefone = dados.Telefone;
        clinica.Email = dados.Email;
        clinica.Ativa = dados.Ativa;

        await _repositorio.AtualizarAsync(clinica);
        return clinica;
    }

    public async Task ExcluirAsync(int id)
    {
        var clinica = await BuscarPorIdAsync(id);
        await _repositorio.RemoverAsync(clinica);
    }

    private async Task ValidarAsync(Clinica clinica)
    {
        if (string.IsNullOrWhiteSpace(clinica.RazaoSocial))
            throw new NegocioException("A razão social é obrigatória.");

        if (string.IsNullOrWhiteSpace(clinica.NomeFantasia))
            throw new NegocioException("O nome fantasia é obrigatório.");

        // Aceitamos o CNPJ com ou sem máscara, mas guardamos só os dígitos.
        clinica.Cnpj = ApenasDigitos(clinica.Cnpj);

        if (clinica.Cnpj.Length != 14)
            throw new NegocioException("O CNPJ deve ter 14 dígitos.");

        // Nem todo banco garante CNPJ único (o em memória não garante), então a regra é conferida aqui.
        var clinicaComMesmoCnpj = await _repositorio.BuscarPorCnpjAsync(clinica.Cnpj);

        if (clinicaComMesmoCnpj is not null && clinicaComMesmoCnpj.Id != clinica.Id)
            throw new NegocioException("Já existe uma clínica cadastrada com este CNPJ.");
    }

    private static string ApenasDigitos(string? texto)
    {
        return new string((texto ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
    }
}
