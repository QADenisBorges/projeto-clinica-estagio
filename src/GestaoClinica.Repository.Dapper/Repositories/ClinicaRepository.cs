using Dapper;
using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Interfaces;
using GestaoClinica.Repository.Dapper.Context;

namespace GestaoClinica.Repository.Dapper.Repositories;

// Mesmo contrato do Repository.EF, mas aqui o SQL é escrito à mão e o Dapper
// só converte o resultado em objetos. Nenhuma regra de negócio fica aqui.
public class ClinicaRepository : IClinicaRepository
{
    private const string Colunas = "Id, Cnpj, RazaoSocial, NomeFantasia, Telefone, Email, Ativa";

    private readonly ConnectionFactory _fabrica;

    public ClinicaRepository(ConnectionFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<List<Clinica>> ListarAsync()
    {
        using var conexao = _fabrica.Criar();

        var clinicas = await conexao.QueryAsync<Clinica>(
            $"SELECT {Colunas} FROM Clinicas ORDER BY Id");

        return clinicas.ToList();
    }

    public async Task<Clinica?> BuscarPorIdAsync(int id)
    {
        using var conexao = _fabrica.Criar();

        // Os valores sempre vão como parâmetro (@id), nunca concatenados no texto do SQL.
        // É isso que protege contra SQL Injection.
        return await conexao.QuerySingleOrDefaultAsync<Clinica>(
            $"SELECT {Colunas} FROM Clinicas WHERE Id = @id",
            new { id });
    }

    public async Task<Clinica?> BuscarPorCnpjAsync(string cnpj)
    {
        using var conexao = _fabrica.Criar();

        return await conexao.QueryFirstOrDefaultAsync<Clinica>(
            $"SELECT {Colunas} FROM Clinicas WHERE Cnpj = @cnpj",
            new { cnpj });
    }

    public async Task AdicionarAsync(Clinica clinica)
    {
        using var conexao = _fabrica.Criar();

        // OUTPUT INSERTED.Id devolve o Id que o SQL Server gerou.
        clinica.Id = await conexao.QuerySingleAsync<int>(
            """
            INSERT INTO Clinicas (Cnpj, RazaoSocial, NomeFantasia, Telefone, Email, Ativa)
            OUTPUT INSERTED.Id
            VALUES (@Cnpj, @RazaoSocial, @NomeFantasia, @Telefone, @Email, @Ativa)
            """,
            Parametros(clinica));
    }

    public async Task AtualizarAsync(Clinica clinica)
    {
        using var conexao = _fabrica.Criar();

        await conexao.ExecuteAsync(
            """
            UPDATE Clinicas
               SET Cnpj = @Cnpj,
                   RazaoSocial = @RazaoSocial,
                   NomeFantasia = @NomeFantasia,
                   Telefone = @Telefone,
                   Email = @Email,
                   Ativa = @Ativa
             WHERE Id = @Id
            """,
            Parametros(clinica));
    }

    public async Task RemoverAsync(Clinica clinica)
    {
        using var conexao = _fabrica.Criar();
        await conexao.OpenAsync();

        // Transação: ou apaga a clínica com médicos e pacientes, ou não apaga nada.
        await using var transacao = await conexao.BeginTransactionAsync();

        var parametros = new { clinica.Id };
        await conexao.ExecuteAsync("DELETE FROM Medicos WHERE ClinicaId = @Id", parametros, transacao);
        await conexao.ExecuteAsync("DELETE FROM Pacientes WHERE ClinicaId = @Id", parametros, transacao);
        await conexao.ExecuteAsync("DELETE FROM Clinicas WHERE Id = @Id", parametros, transacao);

        await transacao.CommitAsync();
    }

    // Só os campos que viram coluna. As listas de médicos e pacientes ficam de fora.
    private static object Parametros(Clinica clinica)
    {
        return new
        {
            clinica.Id,
            clinica.Cnpj,
            clinica.RazaoSocial,
            clinica.NomeFantasia,
            clinica.Telefone,
            clinica.Email,
            clinica.Ativa
        };
    }
}
