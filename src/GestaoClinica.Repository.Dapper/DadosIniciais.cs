using Dapper;
using Microsoft.Data.SqlClient;

namespace GestaoClinica.Repository.Dapper;

// Cria as tabelas e cadastra os mesmos dados de exemplo do projeto Repository.EF.
// Aqui o banco é um SQL Server de verdade, então os dados continuam lá depois que a aplicação para.
public static class DadosIniciais
{
    // Mesmos tamanhos e relacionamentos da pasta Mappings do Repository.EF.
    // Diferença: aqui o banco não apaga os filhos sozinho (sem ON DELETE CASCADE);
    // quem apaga os pacientes e as ligações com médicos é o ClinicaRepository, dentro de uma transação.
    private const string CriarTabelas =
        """
        IF OBJECT_ID('Clinicas') IS NULL
        BEGIN
            CREATE TABLE Clinicas (
                Id           INT IDENTITY(1,1) PRIMARY KEY,
                Cnpj         NVARCHAR(14)  NOT NULL UNIQUE,
                RazaoSocial  NVARCHAR(200) NOT NULL,
                NomeFantasia NVARCHAR(200) NOT NULL,
                Telefone     NVARCHAR(20)  NULL,
                Email        NVARCHAR(200) NULL,
                Ativa        BIT           NOT NULL
            );

            CREATE TABLE Medicos (
                Id            INT IDENTITY(1,1) PRIMARY KEY,
                Nome          NVARCHAR(200) NOT NULL,
                Crm           NVARCHAR(20)  NOT NULL,
                Especialidade NVARCHAR(100) NOT NULL
            );

            -- Tabela de ligação do N—N: cada linha diz "este médico atende nesta clínica".
            CREATE TABLE ClinicaMedico (
                ClinicaId INT NOT NULL REFERENCES Clinicas(Id),
                MedicoId  INT NOT NULL REFERENCES Medicos(Id),
                PRIMARY KEY (ClinicaId, MedicoId)
            );

            CREATE TABLE Pacientes (
                Id             INT IDENTITY(1,1) PRIMARY KEY,
                Nome           NVARCHAR(200) NOT NULL,
                Cpf            NVARCHAR(11)  NOT NULL,
                DataNascimento DATE          NOT NULL,
                ClinicaId      INT NOT NULL REFERENCES Clinicas(Id)
            );
        END
        """;

    // Os Ids não são informados: em um banco novo, o SQL Server gera 1, 2 e 3 na ordem dos INSERTs.
    private const string InserirDados =
        """
        INSERT INTO Clinicas (Cnpj, RazaoSocial, NomeFantasia, Telefone, Email, Ativa) VALUES
            ('11222333000181', N'Clínica Vida Plena Serviços Médicos Ltda', N'Vida Plena', '(11) 3333-1000', 'contato@vidaplena.com.br', 1),
            ('22333444000172', N'Centro Médico Bem Estar S.A.', N'Bem Estar', '(21) 4444-2000', 'atendimento@bemestar.com.br', 1),
            ('33444555000163', N'Clínica Sorriso Odontologia Ltda', N'Sorriso', '(31) 5555-3000', 'contato@clinicasorriso.com.br', 0);

        INSERT INTO Medicos (Nome, Crm, Especialidade) VALUES
            (N'Ana Souza',       'CRM-SP 123456', N'Cardiologia'),
            (N'Bruno Lima',      'CRM-SP 234567', N'Ortopedia'),
            (N'Eduardo Martins', 'CRM-RJ 345678', N'Pediatria'),
            (N'Helena Costa',    'CRO-MG 45678',  N'Odontologia');

        -- Ana Souza atende em duas clínicas, para mostrar o relacionamento N—N.
        INSERT INTO ClinicaMedico (ClinicaId, MedicoId)
        SELECT c.Id, m.Id
          FROM (VALUES
                ('11222333000181', 'CRM-SP 123456'),
                ('11222333000181', 'CRM-SP 234567'),
                ('22333444000172', 'CRM-SP 123456'),
                ('22333444000172', 'CRM-RJ 345678'),
                ('33444555000163', 'CRO-MG 45678')
               ) AS l (Cnpj, Crm)
          JOIN Clinicas c ON c.Cnpj = l.Cnpj
          JOIN Medicos  m ON m.Crm  = l.Crm;

        INSERT INTO Pacientes (Nome, Cpf, DataNascimento, ClinicaId)
        SELECT p.Nome, p.Cpf, p.DataNascimento, c.Id
          FROM (VALUES
                (N'Carlos Pereira', '11122233344', '1985-03-12', '11222333000181'),
                (N'Daniela Rocha',  '22233344455', '1992-07-30', '11222333000181'),
                (N'Fernanda Alves', '33344455566', '2010-11-05', '22333444000172'),
                (N'Gabriel Nunes',  '44455566677', '1978-01-22', '22333444000172'),
                (N'Igor Santos',    '55566677788', '2001-09-14', '33444555000163')
               ) AS p (Nome, Cpf, DataNascimento, Cnpj)
          JOIN Clinicas c ON c.Cnpj = p.Cnpj;
        """;

    public static void Popular(SqlConnection conexao)
    {
        conexao.Execute(CriarTabelas);

        // Evita duplicar os dados se o método for chamado mais de uma vez.
        var jaTemDados = conexao.ExecuteScalar<int>("SELECT COUNT(*) FROM Clinicas") > 0;
        if (jaTemDados)
            return;

        conexao.Execute(InserirDados);
    }
}
