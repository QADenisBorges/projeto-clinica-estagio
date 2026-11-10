namespace GestaoClinica.Domain.Entities.Base;

// Classe mãe de todas as entidades. Ela garante que toda entidade tem Id,
// e é isso que permite ao repositório base buscar qualquer uma delas pelo Id.
public abstract class Entity
{
    public int Id { get; set; }
}
