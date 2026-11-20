using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Exceptions;
using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Domain.Interfaces.Repositories;

namespace GestaoClinica.Domain.Services;

// Todas as regras de negócio de clínica moram aqui.
// Quando uma regra é quebrada, lançamos NegocioException e a API cuida da resposta.
public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _repositorio;

    public PacienteService(IPacienteRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Paciente>> ListarAsync()
    {
        return _repositorio.ListarAsync();
    }

    public async Task<Paciente> BuscarPorIdAsync(int id)
    {
        var paciente = await _repositorio.BuscarPorIdAsync(id);

        if (paciente is null)
            throw new RegistroNaoEncontradoException("Paciente não encontrado.");

        return paciente;
    }

    public async Task<Paciente> CriarAsync(Paciente Paciente)
    {
        await ValidarAsync(Paciente);

        await _repositorio.AdicionarAsync(Paciente);
        return Paciente;
    }

    public async Task<Paciente> AtualizarAsync(int id, Paciente dados)
    {
        var paciente = await BuscarPorIdAsync(id);

        // Validamos antes de copiar para não alterar a clínica salva com dados inválidos.
        dados.Id = id;
        await ValidarAsync(dados);

        paciente.Cpf = dados.Cpf;
        paciente.DataNascimento = dados.DataNascimento;
        paciente.Nome = dados.Nome;

        await _repositorio.AtualizarAsync(paciente);
        return paciente;
    }

    public async Task ExcluirAsync(int id)
    {
        var paciente = await BuscarPorIdAsync(id);
        await _repositorio.RemoverAsync(paciente);
    }

    private async Task ValidarAsync(Paciente paciente)
    {
        // Validar nome
        if (string.IsNullOrWhiteSpace(paciente.Nome))
            throw new ArgumentException("O nome do paciente é obrigatório.");

        // Validar CPF
        if (string.IsNullOrWhiteSpace(paciente.Cpf))
            throw new ArgumentException("O CPF do paciente é obrigatório.");

        // Validar quantidade de números do CPF
        string cpfLimpo = paciente.Cpf.Replace(".", "").Replace("-", "");

        if (cpfLimpo.Length != 11 || !cpfLimpo.All(char.IsDigit))
            throw new ArgumentException("O CPF deve conter 11 números.");

        // Validar data de nascimento
        if (paciente.DataNascimento == default)
            throw new ArgumentException("A data de nascimento é obrigatória.");

        if (paciente.DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("A data de nascimento não pode ser futura.");

        // Nem todo banco garante CNPJ único (o em memória não garante), então a regra é conferida aqui.
        var pacienteComMesmoCpf = await _repositorio.BuscarPorCpfAsync(paciente.Cpf);

        if (pacienteComMesmoCpf is not null && pacienteComMesmoCpf.Id != paciente.Id)
            throw new NegocioException("Já existe um Paciente cadastrado com este CNPJ.");
    }


