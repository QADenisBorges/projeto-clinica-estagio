using GestaoClinica.Domain.Entities;
using GestaoClinica.Repository.EF.Context;

namespace GestaoClinica.Repository.EF;

// Dados de exemplo para a API já subir com conteúdo.
// Como o banco é em memória, tudo isso é recriado a cada vez que a aplicação inicia.
public static class DadosIniciais
{
    public static void Popular(ClinicaDbContext contexto)
    {
        // Evita duplicar os dados se o método for chamado mais de uma vez.
        if (contexto.Clinicas.Any())
            return;

        contexto.Clinicas.AddRange(
            new Clinica
            {
                Id = 1,
                Cnpj = "11222333000181",
                RazaoSocial = "Clínica Vida Plena Serviços Médicos Ltda",
                NomeFantasia = "Vida Plena",
                Telefone = "(11) 3333-1000",
                Email = "contato@vidaplena.com.br",
                Ativa = true,
                Medicos =
                {
                    new Medico { Nome = "Ana Souza", Crm = "CRM-SP 123456", Especialidade = "Cardiologia" },
                    new Medico { Nome = "Bruno Lima", Crm = "CRM-SP 234567", Especialidade = "Ortopedia" }
                },
                Pacientes =
                {
                    new Paciente { Nome = "Carlos Pereira", Cpf = "11122233344", DataNascimento = new DateOnly(1985, 3, 12) },
                    new Paciente { Nome = "Daniela Rocha", Cpf = "22233344455", DataNascimento = new DateOnly(1992, 7, 30) }
                }
            },
            new Clinica
            {
                Id = 2,
                Cnpj = "22333444000172",
                RazaoSocial = "Centro Médico Bem Estar S.A.",
                NomeFantasia = "Bem Estar",
                Telefone = "(21) 4444-2000",
                Email = "atendimento@bemestar.com.br",
                Ativa = true,
                Medicos =
                {
                    new Medico { Nome = "Eduardo Martins", Crm = "CRM-RJ 345678", Especialidade = "Pediatria" }
                },
                Pacientes =
                {
                    new Paciente { Nome = "Fernanda Alves", Cpf = "33344455566", DataNascimento = new DateOnly(2010, 11, 5) },
                    new Paciente { Nome = "Gabriel Nunes", Cpf = "44455566677", DataNascimento = new DateOnly(1978, 1, 22) }
                }
            },
            new Clinica
            {
                Id = 3,
                Cnpj = "33444555000163",
                RazaoSocial = "Clínica Sorriso Odontologia Ltda",
                NomeFantasia = "Sorriso",
                Telefone = "(31) 5555-3000",
                Email = "contato@clinicasorriso.com.br",
                // Uma clínica inativa para o exemplo ter os dois casos.
                Ativa = false,
                Medicos =
                {
                    new Medico { Nome = "Helena Costa", Crm = "CRO-MG 45678", Especialidade = "Odontologia" }
                },
                Pacientes =
                {
                    new Paciente { Nome = "Igor Santos", Cpf = "55566677788", DataNascimento = new DateOnly(2001, 9, 14) }
                }
            });

        contexto.SaveChanges();
    }
}
