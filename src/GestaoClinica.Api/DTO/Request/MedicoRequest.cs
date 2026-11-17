using GestaoClinica.Domain.Entities;

namespace GestaoClinica.Api.DTO.Request
{
    public class MedicoRequest
    {
        public required string Nome { get; set; }
        public required string Crm { get; set; }
        public string? Especialidade { get; set; }
        public required string PontoReferencia { get; set; }
        public string? Complento { get; set; }

        public Medico ParaEntidade()
        {
            return new Medico
            {
                Nome = Nome,
                Crm = Crm,
                Especialidade = Especialidade
            };
        }
    }
}
