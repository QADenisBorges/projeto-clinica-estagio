using GestaoClinica.Domain.Entities;
using GestaoClinica.Domain.Interfaces.Base;

namespace GestaoClinica.Domain.Interfaces;

// O Domain diz O QUE precisa do banco; os projetos Repository.* decidem COMO fazer.
// Assim o Domain não depende de Entity Framework, Dapper ou qualquer outra tecnologia.
//
// Listar, buscar por Id, adicionar, atualizar e remover vêm de IRepositoryBase.
// Aqui fica só o que é específico de clínica.
public interface IClinicaRepository : IRepositoryBase<Clinica>
{
    Task<Clinica?> BuscarPorCnpjAsync(string cnpj);
}
