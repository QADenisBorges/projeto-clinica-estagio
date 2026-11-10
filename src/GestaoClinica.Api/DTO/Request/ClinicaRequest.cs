using GestaoClinica.Domain.Entities;

namespace GestaoClinica.Api.DTO.Request;

// O que o cliente envia no POST e no PUT.
// Os textos aceitam nulo de propósito: quem valida e devolve a mensagem é o ClinicaService.
public class ClinicaRequest
{
    public string? Cnpj { get; set; }
    public string? RazaoSocial { get; set; }
    public string? NomeFantasia { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativa { get; set; } = true;

    public Clinica ParaEntidade()
    {
        return new Clinica
        {
            Cnpj = Cnpj ?? string.Empty,
            RazaoSocial = RazaoSocial ?? string.Empty,
            NomeFantasia = NomeFantasia ?? string.Empty,
            Telefone = Telefone,
            Email = Email,
            Ativa = Ativa
        };
    }
}
