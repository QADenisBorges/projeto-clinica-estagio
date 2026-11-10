using GestaoClinica.Domain.Entities.Base;

namespace GestaoClinica.Domain.Entities;

// Por enquanto o médico só existe como entidade mapeada: não tem repositório, service nem endpoint.
// O README tem o passo a passo para criar. O Id vem da classe Entity.
public class Medico : Entity
{
    public string Nome { get; set; } = string.Empty;
    public string Crm { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;

    // Todo médico pertence a uma clínica. (1-N)
    public int ClinicaId { get; set; }
    public Clinica? Clinica { get; set; }
}
