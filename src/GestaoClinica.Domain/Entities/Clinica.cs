using GestaoClinica.Domain.Entities.Base;

namespace GestaoClinica.Domain.Entities;

// O Id vem da classe Entity.
public class Clinica : Entity
{
    // Guardado sempre só com os 14 dígitos, sem pontos, barra ou traço.
    public string Cnpj { get; set; } = string.Empty;

    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Telefone { get; set; } = null;
    public string? Email { get; set; } = null;
    public bool Ativa { get; set; } = true;

    // Clínica N—N Médico: a clínica tem vários médicos, e o mesmo médico pode estar em várias clínicas.
    public List<Medico> Medicos { get; set; } = new();

    // Clínica 1—N Paciente: cada paciente pertence a uma clínica só.
    public List<Paciente> Pacientes { get; set; } = new();
}
