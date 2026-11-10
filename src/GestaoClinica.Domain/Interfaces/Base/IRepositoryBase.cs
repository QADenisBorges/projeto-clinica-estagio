using GestaoClinica.Domain.Entities.Base;

namespace GestaoClinica.Domain.Interfaces.Base;

// Operações que todo repositório tem, para qualquer entidade (o T).
// A interface de cada entidade herda desta e acrescenta só o que for específico dela.
// Exemplo: IClinicaRepository acrescenta a busca por CNPJ.
public interface IRepositoryBase<T> where T : Entity
{
    Task<List<T>> ListarAsync();
    Task<T?> BuscarPorIdAsync(int id);
    Task AdicionarAsync(T entidade);
    Task AtualizarAsync(T entidade);
    Task RemoverAsync(T entidade);
}
