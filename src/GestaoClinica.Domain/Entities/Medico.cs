using GestaoClinica.Domain.Entities.Base;

namespace GestaoClinica.Domain.Entities;

// Por enquanto o médico só existe como entidade mapeada: não tem repositório, service nem endpoint.
// O README tem o passo a passo para criar. O Id vem da classe Entity.
public class Medico : Entity
{
    public string Nome { get; set; } = string.Empty;
    public string Crm { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;

    // Um médico pode atender em várias clínicas (N—N).
    public List<Clinica> Clinicas { get; set; } = new();
}
