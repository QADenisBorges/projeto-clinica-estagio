using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Repository.EF.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestaoClinica.Tests;

// Testes de ponta a ponta: sobem a API de verdade em memória e fazem chamadas HTTP.
// O xUnit cria uma instância da classe por teste, então cada teste tem sua API e seu banco.
public class ClinicasApiTests : IDisposable
{
    private readonly ApiDeTeste _api = new();
    private readonly HttpClient _cliente;

    public ClinicasApiTests()
    {
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    [Fact]
    public async Task Listar_devolve_200_com_as_clinicas_do_seed()
    {
        var resposta = await _cliente.GetAsync("/api/clinicas");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var clinicas = await LerJsonAsync(resposta);
        Assert.Equal(3, clinicas.GetArrayLength());
        Assert.Equal("Vida Plena", clinicas[0].GetProperty("nomeFantasia").GetString());
    }

    [Fact]
    public async Task Resposta_tem_so_os_campos_da_clinica_sem_medicos_nem_pacientes()
    {
        var resposta = await _cliente.GetAsync("/api/clinicas/1");

        var clinica = await LerJsonAsync(resposta);
        var campos = clinica.EnumerateObject().Select(p => p.Name).OrderBy(n => n).ToArray();
        Assert.Equal(
            new[] { "ativa", "cnpj", "email", "id", "nomeFantasia", "razaoSocial", "telefone" },
            campos);
    }

    [Fact]
    public async Task Buscar_existente_devolve_200()
    {
        var resposta = await _cliente.GetAsync("/api/clinicas/1");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var clinica = await LerJsonAsync(resposta);
        Assert.Equal(1, clinica.GetProperty("id").GetInt32());
        Assert.Equal("11222333000181", clinica.GetProperty("cnpj").GetString());
    }

    [Fact]
    public async Task Buscar_inexistente_devolve_404_com_mensagem()
    {
        var resposta = await _cliente.GetAsync("/api/clinicas/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("Clínica não encontrada.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Criar_valida_devolve_201_com_location()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clinicas", RequestValido("12345678000190"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var clinica = await LerJsonAsync(resposta);
        var id = clinica.GetProperty("id").GetInt32();
        Assert.True(id > 3);
        Assert.Equal("contato@nova.com.br", clinica.GetProperty("email").GetString());
        Assert.NotNull(resposta.Headers.Location);
        Assert.EndsWith($"/api/clinicas/{id}", resposta.Headers.Location.ToString(), StringComparison.OrdinalIgnoreCase);

        // O Location precisa levar mesmo até a clínica criada.
        var busca = await _cliente.GetAsync(resposta.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
    }

    [Fact]
    public async Task Criar_com_cnpj_com_mascara_devolve_201_e_salva_so_digitos()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clinicas", RequestValido("12.345.678/0001-90"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var clinica = await LerJsonAsync(resposta);
        Assert.Equal("12345678000190", clinica.GetProperty("cnpj").GetString());
    }

    [Fact]
    public async Task Criar_com_cnpj_invalido_devolve_400_com_mensagem()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clinicas", RequestValido("123"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("O CNPJ deve ter 14 dígitos.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Criar_sem_razao_social_devolve_400_com_mensagem()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clinicas",
            new { cnpj = "12345678000190", nomeFantasia = "Nova" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("A razão social é obrigatória.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Criar_sem_nome_fantasia_devolve_400_com_mensagem()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clinicas",
            new { cnpj = "12345678000190", razaoSocial = "Clínica Nova Ltda", nomeFantasia = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("O nome fantasia é obrigatório.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Criar_com_cnpj_duplicado_devolve_400_com_mensagem()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clinicas", RequestValido("11.222.333/0001-81"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Já existe uma clínica cadastrada com este CNPJ.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Atualizar_valido_devolve_200_com_dados_novos()
    {
        var request = new
        {
            cnpj = "11222333000181",
            razaoSocial = "Razão Nova Ltda",
            nomeFantasia = "Fantasia Nova",
            telefone = "(11) 9999-0000",
            email = "novo@exemplo.com.br",
            ativa = false
        };

        var resposta = await _cliente.PutAsJsonAsync("/api/clinicas/1", request);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var clinica = await LerJsonAsync(resposta);
        Assert.Equal("Fantasia Nova", clinica.GetProperty("nomeFantasia").GetString());
        Assert.False(clinica.GetProperty("ativa").GetBoolean());

        // Confere que a mudança foi salva, e não só devolvida.
        var salva = await LerJsonAsync(await _cliente.GetAsync("/api/clinicas/1"));
        Assert.Equal("Razão Nova Ltda", salva.GetProperty("razaoSocial").GetString());
        Assert.Equal("(11) 9999-0000", salva.GetProperty("telefone").GetString());
        Assert.Equal("novo@exemplo.com.br", salva.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Criar_sem_informar_ativa_devolve_201_com_ativa_true()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clinicas",
            new { cnpj = "12345678000190", razaoSocial = "Clínica Nova Ltda", nomeFantasia = "Nova" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.True((await LerJsonAsync(resposta)).GetProperty("ativa").GetBoolean());
    }

    [Fact]
    public async Task Criar_com_json_malformado_devolve_400_com_mensagem()
    {
        var corpo = new StringContent("{ \"cnpj\": ", System.Text.Encoding.UTF8, "application/json");

        var resposta = await _cliente.PostAsync("/api/clinicas", corpo);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Corpo da requisição inválido.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Raiz_redireciona_para_o_swagger()
    {
        using var cliente = _api.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resposta = await cliente.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.Equal("/swagger", resposta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Atualizar_sem_razao_social_devolve_400_com_mensagem()
    {
        var resposta = await _cliente.PutAsJsonAsync("/api/clinicas/1",
            new { cnpj = "11222333000181", razaoSocial = " ", nomeFantasia = "Vida Plena" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("A razão social é obrigatória.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Atualizar_com_cnpj_de_outra_clinica_devolve_400_e_nao_altera()
    {
        var resposta = await _cliente.PutAsJsonAsync("/api/clinicas/1", RequestValido("22333444000172"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Já existe uma clínica cadastrada com este CNPJ.", await LerMensagemAsync(resposta));

        var salva = await LerJsonAsync(await _cliente.GetAsync("/api/clinicas/1"));
        Assert.Equal("11222333000181", salva.GetProperty("cnpj").GetString());
    }

    [Fact]
    public async Task Atualizar_inexistente_devolve_404_com_mensagem()
    {
        var resposta = await _cliente.PutAsJsonAsync("/api/clinicas/999", RequestValido("12345678000190"));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("Clínica não encontrada.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Excluir_devolve_204_e_remove_a_clinica_com_medicos_e_pacientes()
    {
        var resposta = await _cliente.DeleteAsync("/api/clinicas/1");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync("/api/clinicas/1")).StatusCode);

        using var escopo = _api.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        Assert.False(await contexto.Medicos.AnyAsync(m => m.ClinicaId == 1));
        Assert.False(await contexto.Pacientes.AnyAsync(p => p.ClinicaId == 1));
        Assert.True(await contexto.Medicos.AnyAsync(m => m.ClinicaId == 2));
    }

    [Fact]
    public async Task Excluir_inexistente_devolve_404_com_mensagem()
    {
        var resposta = await _cliente.DeleteAsync("/api/clinicas/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("Clínica não encontrada.", await LerMensagemAsync(resposta));
    }

    [Fact]
    public async Task Erro_inesperado_devolve_500_com_mensagem_generica_sem_detalhes()
    {
        // Troca o repositório por um que sempre quebra, para simular um erro não previsto.
        using var apiQuebrada = _api.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.Replace(ServiceDescriptor.Scoped<IClinicaRepository, RepositoryQuebrado>())));
        using var cliente = apiQuebrada.CreateClient();

        var resposta = await cliente.GetAsync("/api/clinicas");

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Equal("Ocorreu um erro inesperado. Tente novamente mais tarde.",
            JsonDocument.Parse(corpo).RootElement.GetProperty("mensagem").GetString());
        Assert.DoesNotContain("falha secreta", corpo);
        Assert.DoesNotContain("RepositoryQuebrado", corpo);
    }

    [Fact]
    public async Task Swagger_lista_os_5_endpoints_de_clinicas()
    {
        var resposta = await _cliente.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var caminhos = (await LerJsonAsync(resposta)).GetProperty("paths");
        var operacoes = caminhos.EnumerateObject()
            .SelectMany(caminho => caminho.Value.EnumerateObject()
                .Select(operacao => $"{operacao.Name.ToUpperInvariant()} {caminho.Name}"))
            .OrderBy(o => o)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "DELETE /api/clinicas/{id}",
                "GET /api/clinicas",
                "GET /api/clinicas/{id}",
                "POST /api/clinicas",
                "PUT /api/clinicas/{id}"
            },
            operacoes);
    }

    private static object RequestValido(string cnpj)
    {
        return new
        {
            cnpj,
            razaoSocial = "Clínica Nova Ltda",
            nomeFantasia = "Nova",
            telefone = "(41) 3030-4000",
            email = "contato@nova.com.br",
            ativa = true
        };
    }

    private static async Task<JsonElement> LerJsonAsync(HttpResponseMessage resposta)
    {
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;
    }

    private static async Task<string?> LerMensagemAsync(HttpResponseMessage resposta)
    {
        Assert.Equal("application/json", resposta.Content.Headers.ContentType?.MediaType);
        return (await LerJsonAsync(resposta)).GetProperty("mensagem").GetString();
    }

    // Sobe a API com um banco em memória de nome único, para um teste não enxergar os dados do outro.
    private class ApiDeTeste : WebApplicationFactory<Program>
    {
        private readonly string _nomeDoBanco = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ClinicaDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ClinicaDbContext>>();
                services.AddDbContext<ClinicaDbContext>(options => options.UseInMemoryDatabase(_nomeDoBanco));
            });
        }
    }

    private class RepositoryQuebrado : IClinicaRepository
    {
        public Task<List<Clinica>> ListarAsync() => throw new InvalidOperationException("falha secreta");
        public Task<Clinica?> BuscarPorIdAsync(int id) => throw new InvalidOperationException("falha secreta");
        public Task<Clinica?> BuscarPorCnpjAsync(string cnpj) => throw new InvalidOperationException("falha secreta");
        public Task AdicionarAsync(Clinica clinica) => throw new InvalidOperationException("falha secreta");
        public Task AtualizarAsync(Clinica clinica) => throw new InvalidOperationException("falha secreta");
        public Task RemoverAsync(Clinica clinica) => throw new InvalidOperationException("falha secreta");
    }
}
