using GestaoClinica.Domain.Entities;

namespace GestaoClinica.Api.DTO.Response
{
    public class MedicoResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Crm {  get; set; } = string.Empty;
        public string Especialidae { get; set; } = string.Empty;

        public static MedicoResponse DeEntidade(Medico medico)
        {
            return new MedicoResponse
            {
                Id = medico.Id,
                Crm = medico.Crm,
                Name = medico.Nome,
                Especialidae = medico.Especialidade
            };
        }

    }
}
