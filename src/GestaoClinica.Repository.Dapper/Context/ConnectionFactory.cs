using Microsoft.Data.SqlClient;

namespace GestaoClinica.Repository.Dapper.Context;

// Guarda a connection string e cria conexões com o SQL Server.
// Faz o papel que o DbContext faz no projeto Repository.EF.
public class ConnectionFactory
{
    private readonly string _connectionString;

    public ConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Cada operação usa uma conexão nova e a fecha no final (using).
    // O SQL Server reaproveita as conexões por baixo dos panos, então isso é barato.
    public SqlConnection Criar()
    {
        return new SqlConnection(_connectionString);
    }
}
