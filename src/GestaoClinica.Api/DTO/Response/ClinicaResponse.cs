using GestaoClinica.Domain.Entities;

namespace GestaoClinica.Api.DTO.Response;

// O que a API devolve. Não tem as listas de médicos e pacientes:
// assim a resposta fica pequena e não entra em ciclo (clínica → médico → clínica...).
public class ClinicaResponse
{
    public int Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativa { get; set; }

    public static ClinicaResponse DeEntidade(Clinica clinica)
    {
        return new ClinicaResponse
        {
            Id = clinica.Id,
            Cnpj = clinica.Cnpj,
            RazaoSocial = clinica.RazaoSocial,
            NomeFantasia = clinica.NomeFantasia,
            Telefone = clinica.Telefone,
            Email = clinica.Email,
            Ativa = clinica.Ativa
        };
    }
}
