using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Repository.Dapper.Context;
using GestaoClinica.Repository.Dapper.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoClinica.Repository.Dapper;

// Tudo que este projeto precisa registrar na injeção de dependência fica aqui.
// Os métodos têm os mesmos nomes do Repository.EF para a troca na Api ser simples.
public static class DependencyInjection
{
    // O Dapper precisa de um banco de verdade, então a connection string é obrigatória.
    public static IServiceCollection AddRepository(this IServiceCollection services, string? connectionString)
    {
        // Falha logo na inicialização, com mensagem clara, se a connection string não foi configurada.
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton(new ConnectionFactory(connectionString));

        // Quem pedir IClinicaRepository recebe ClinicaRepository.
        services.AddScoped<IClinicaRepository, ClinicaRepository>();

        return services;
    }

    // Cria as tabelas e cadastra os dados de exemplo. Deve ser chamado uma vez, quando a aplicação inicia.
    public static void PopularDadosIniciais(this IServiceProvider services)
    {
        var fabrica = services.GetRequiredService<ConnectionFactory>();

        using var conexao = fabrica.Criar();
        DadosIniciais.Popular(conexao);
    }
}
