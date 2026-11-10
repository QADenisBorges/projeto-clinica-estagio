using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Repository.EF.Context;
using GestaoClinica.Repository.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoClinica.Repository.EF;

// Tudo que este projeto precisa registrar na injeção de dependência fica aqui.
// Assim a Api só chama AddRepository() e não precisa conhecer o Entity Framework.
public static class DependencyInjection
{
    public static IServiceCollection AddRepository(this IServiceCollection services)
    {
        // Banco em memória: não precisa instalar nada e os dados somem quando a aplicação para.
        // Para usar um SQL Server de verdade não basta trocar esta linha. Os passos são:
        //   1. instalar o pacote Microsoft.EntityFrameworkCore.SqlServer;
        //   2. colocar a connection string "ClinicaDb" no appsettings.json da Api;
        //   3. receber a connection string neste método e trocar a linha abaixo por
        //        options.UseSqlServer(connectionString);
        //   4. criar o banco (EnsureCreated ou migrations);
        //   5. tirar os Ids fixos de DadosIniciais (o SQL Server gera os Ids).
        services.AddDbContext<ClinicaDbContext>(options =>
            options.UseInMemoryDatabase("ClinicaDb"));

        // Quem pedir IClinicaRepository recebe ClinicaRepository.
        services.AddScoped<IClinicaRepository, ClinicaRepository>();

        // Repositório novo? Registre aqui, uma linha por entidade. Exemplo:
        //   services.AddScoped<IMedicoRepository, MedicoRepository>();
        //   services.AddScoped<IPacienteRepository, PacienteRepository>();

        return services;
    }

    // Cadastra os dados de exemplo. Deve ser chamado uma vez, quando a aplicação inicia.
    public static void PopularDadosIniciais(this IServiceProvider services)
    {
        using var escopo = services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        DadosIniciais.Popular(contexto);
    }
}
