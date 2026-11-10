using GestaoClinica.Domain.Entities.Base;

namespace GestaoClinica.Domain.Entities;

// Por enquanto o paciente só existe como entidade mapeada: não tem repositório, service nem endpoint.
// O README tem o passo a passo para criar. O Id vem da classe Entity.
public class Paciente : Entity
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }

    // Todo paciente pertence a uma clínica.
    public int ClinicaId { get; set; }
    public Clinica? Clinica { get; set; }
}
