using GestaoClinica.Domain.Entities.Base;

namespace GestaoClinica.Domain.Entities;

// O Id vem da classe Entity.
// Commit do Iago
// Commit do Guylherme

public class Clinica : Entity
{
    // Guardado sempre só com os 14 dígitos, sem pontos, barra ou traço.
    public string Cnpj { get; set; } = string.Empty;

    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Telefone { get; set; } = null;
    public string? Email { get; set; } = null;
    public bool Ativa { get; set; } = true;

    // Uma clínica tem vários médicos e vários pacientes (1—N).
    public List<Medico> Medicos { get; set; } = new();
    public List<Paciente> Pacientes { get; set; } = new();
}
