using GestaoClinica.Domain.Entities;
using GestaoClinica.Repository.EF.Context;
using GestaoClinica.Repository.EF.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace GestaoClinica.Tests;

// Prova que o RepositoryBase serve para qualquer entidade, não só para Clinica.
// O MedicoRepository abaixo é tudo que é preciso escrever para ganhar as cinco operações.
public class RepositoryBaseTests : IDisposable
{
    private readonly ClinicaDbContext _contexto;
    private readonly MedicoRepository _repositorio;

    public RepositoryBaseTests()
    {
        // Um banco em memória só deste teste, para um teste não atrapalhar o outro.
        var opcoes = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _contexto = new ClinicaDbContext(opcoes);
        _contexto.Clinicas.Add(new Clinica
        {
            Id = 1,
            Cnpj = "11222333000181",
            RazaoSocial = "Clínica Teste Ltda",
            NomeFantasia = "Teste",
            Medicos =
            {
                new Medico { Id = 2, Nome = "Bruno Lima", Crm = "CRM-SP 2", Especialidade = "Ortopedia" },
                new Medico { Id = 1, Nome = "Ana Souza", Crm = "CRM-SP 1", Especialidade = "Cardiologia" }
            }
        });
        _contexto.SaveChanges();
        _contexto.ChangeTracker.Clear();

        _repositorio = new MedicoRepository(_contexto);
    }

    public void Dispose()
    {
        _contexto.Dispose();
    }

    [Fact]
    public async Task Listar_devolve_todos_em_ordem_de_id()
    {
        var medicos = await _repositorio.ListarAsync();

        Assert.Equal(new[] { 1, 2 }, medicos.Select(m => m.Id));
    }

    [Fact]
    public async Task BuscarPorId_devolve_a_entidade_ou_nulo()
    {
        var existente = await _repositorio.BuscarPorIdAsync(2);
        var inexistente = await _repositorio.BuscarPorIdAsync(999);

        Assert.Equal("Bruno Lima", existente?.Nome);
        Assert.Null(inexistente);
    }

    [Fact]
    public async Task Adicionar_salva_e_preenche_o_id()
    {
        var novo = new Medico { Nome = "Carla Dias", Crm = "CRM-SP 3", Especialidade = "Pediatria" };

        await _repositorio.AdicionarAsync(novo);

        Assert.True(novo.Id > 0);
        Assert.Equal(3, (await _repositorio.ListarAsync()).Count);
    }

    [Fact]
    public async Task Atualizar_grava_as_mudancas()
    {
        var medico = await _repositorio.BuscarPorIdAsync(1);
        medico!.Especialidade = "Clínica Geral";

        await _repositorio.AtualizarAsync(medico);
        _contexto.ChangeTracker.Clear();

        Assert.Equal("Clínica Geral", (await _repositorio.BuscarPorIdAsync(1))?.Especialidade);
    }

    [Fact]
    public async Task Remover_apaga_so_a_entidade_informada()
    {
        var medico = await _repositorio.BuscarPorIdAsync(1);

        await _repositorio.RemoverAsync(medico!);

        Assert.Null(await _repositorio.BuscarPorIdAsync(1));
        Assert.NotNull(await _repositorio.BuscarPorIdAsync(2));
    }

    private class MedicoRepository : RepositoryBase<Medico>
    {
        public MedicoRepository(ClinicaDbContext contexto) : base(contexto)
        {
        }
    }
}
