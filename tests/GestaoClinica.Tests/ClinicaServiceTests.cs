using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Exceptions;
using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Domain.Services;

namespace GestaoClinica.Tests;

// Testes das regras de negócio, sem banco e sem HTTP.
// O repositório falso guarda as clínicas numa lista em memória.
public class ClinicaServiceTests
{
    private const string CnpjExistente = "11222333000181";

    private readonly RepositoryFalso _repositorio = new();
    private readonly ClinicaService _service;

    public ClinicaServiceTests()
    {
        _repositorio.Clinicas.Add(new Clinica
        {
            Id = 1,
            Cnpj = CnpjExistente,
            RazaoSocial = "Clínica Vida Plena Ltda",
            NomeFantasia = "Vida Plena"
        });
        _repositorio.Clinicas.Add(new Clinica
        {
            Id = 2,
            Cnpj = "22333444000172",
            RazaoSocial = "Centro Médico Bem Estar S.A.",
            NomeFantasia = "Bem Estar"
        });

        _service = new ClinicaService(_repositorio);
    }

    [Fact]
    public async Task Listar_devolve_todas_as_clinicas()
    {
        var clinicas = await _service.ListarAsync();

        Assert.Equal(2, clinicas.Count);
    }

    [Fact]
    public async Task BuscarPorId_devolve_a_clinica_quando_existe()
    {
        var clinica = await _service.BuscarPorIdAsync(1);

        Assert.Equal("Vida Plena", clinica.NomeFantasia);
    }

    [Fact]
    public async Task BuscarPorId_falha_quando_nao_existe()
    {
        var excecao = await Assert.ThrowsAsync<RegistroNaoEncontradoException>(
            () => _service.BuscarPorIdAsync(999));

        Assert.Equal("Clínica não encontrada.", excecao.Message);
    }

    [Fact]
    public async Task Criar_salva_clinica_valida()
    {
        var clinica = await _service.CriarAsync(NovaClinica("12345678000190"));

        Assert.True(clinica.Id > 0);
        Assert.Contains(clinica, _repositorio.Clinicas);
    }

    [Fact]
    public async Task Criar_guarda_cnpj_so_com_digitos()
    {
        var clinica = await _service.CriarAsync(NovaClinica("12.345.678/0001-90"));

        Assert.Equal("12345678000190", clinica.Cnpj);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456780001901")]
    [InlineData("abcdefghijklmn")]
    public async Task Criar_falha_com_cnpj_sem_14_digitos(string cnpj)
    {
        var excecao = await Assert.ThrowsAsync<NegocioException>(
            () => _service.CriarAsync(NovaClinica(cnpj)));

        Assert.Equal("O CNPJ deve ter 14 dígitos.", excecao.Message);
        Assert.Equal(2, _repositorio.Clinicas.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Criar_falha_sem_razao_social(string razaoSocial)
    {
        var clinica = NovaClinica("12345678000190");
        clinica.RazaoSocial = razaoSocial;

        var excecao = await Assert.ThrowsAsync<NegocioException>(() => _service.CriarAsync(clinica));

        Assert.Equal("A razão social é obrigatória.", excecao.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Criar_falha_sem_nome_fantasia(string nomeFantasia)
    {
        var clinica = NovaClinica("12345678000190");
        clinica.NomeFantasia = nomeFantasia;

        var excecao = await Assert.ThrowsAsync<NegocioException>(() => _service.CriarAsync(clinica));

        Assert.Equal("O nome fantasia é obrigatório.", excecao.Message);
    }

    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    public async Task Criar_falha_com_cnpj_de_outra_clinica(string cnpj)
    {
        var excecao = await Assert.ThrowsAsync<NegocioException>(
            () => _service.CriarAsync(NovaClinica(cnpj)));

        Assert.Equal("Já existe uma clínica cadastrada com este CNPJ.", excecao.Message);
    }

    [Fact]
    public async Task Atualizar_altera_os_dados()
    {
        var dados = NovaClinica("99.888.777/0001-66");
        dados.NomeFantasia = "Nome Novo";
        dados.Ativa = false;

        var clinica = await _service.AtualizarAsync(1, dados);

        Assert.Equal(1, clinica.Id);
        Assert.Equal("99888777000166", clinica.Cnpj);
        Assert.Equal("Nome Novo", clinica.NomeFantasia);
        Assert.False(clinica.Ativa);
    }

    [Fact]
    public async Task Atualizar_aceita_manter_o_proprio_cnpj()
    {
        var clinica = await _service.AtualizarAsync(1, NovaClinica(CnpjExistente));

        Assert.Equal(CnpjExistente, clinica.Cnpj);
    }

    [Fact]
    public async Task Atualizar_falha_com_cnpj_de_outra_clinica()
    {
        var excecao = await Assert.ThrowsAsync<NegocioException>(
            () => _service.AtualizarAsync(2, NovaClinica(CnpjExistente)));

        Assert.Equal("Já existe uma clínica cadastrada com este CNPJ.", excecao.Message);
    }

    [Fact]
    public async Task Atualizar_falha_sem_razao_social_e_nao_altera_a_clinica()
    {
        var dados = NovaClinica("99888777000166");
        dados.RazaoSocial = "";

        await Assert.ThrowsAsync<NegocioException>(() => _service.AtualizarAsync(1, dados));

        Assert.Equal(CnpjExistente, _repositorio.Clinicas.Single(c => c.Id == 1).Cnpj);
    }

    [Fact]
    public async Task Atualizar_falha_quando_nao_existe()
    {
        await Assert.ThrowsAsync<RegistroNaoEncontradoException>(
            () => _service.AtualizarAsync(999, NovaClinica("99888777000166")));
    }

    [Fact]
    public async Task Excluir_remove_a_clinica()
    {
        await _service.ExcluirAsync(1);

        Assert.DoesNotContain(_repositorio.Clinicas, c => c.Id == 1);
    }

    [Fact]
    public async Task Excluir_falha_quando_nao_existe()
    {
        var excecao = await Assert.ThrowsAsync<RegistroNaoEncontradoException>(
            () => _service.ExcluirAsync(999));

        Assert.Equal("Clínica não encontrada.", excecao.Message);
    }

    private static Clinica NovaClinica(string cnpj)
    {
        return new Clinica
        {
            Cnpj = cnpj,
            RazaoSocial = "Clínica Nova Ltda",
            NomeFantasia = "Nova"
        };
    }

    private class RepositoryFalso : IClinicaRepository
    {
        public List<Clinica> Clinicas { get; } = new();

        public Task<List<Clinica>> ListarAsync() => Task.FromResult(Clinicas.ToList());

        public Task<Clinica?> BuscarPorIdAsync(int id) =>
            Task.FromResult(Clinicas.FirstOrDefault(c => c.Id == id));

        public Task<Clinica?> BuscarPorCnpjAsync(string cnpj) =>
            Task.FromResult(Clinicas.FirstOrDefault(c => c.Cnpj == cnpj));

        public Task AdicionarAsync(Clinica clinica)
        {
            clinica.Id = Clinicas.Max(c => c.Id) + 1;
            Clinicas.Add(clinica);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Clinica clinica) => Task.CompletedTask;

        public Task RemoverAsync(Clinica clinica)
        {
            Clinicas.Remove(clinica);
            return Task.CompletedTask;
        }
    }
}
