using GestaoClinica.Domain.Entities.Base;
using GestaoClinica.Domain.Interfaces.Base;
using GestaoClinica.Repository.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace GestaoClinica.Repository.EF.Repositories.Base;

// Implementação das operações comuns para qualquer entidade (o T).
// O repositório de cada entidade herda desta classe e já ganha tudo pronto.
// Os métodos são "virtual" para que o filho possa trocar um deles quando precisar
// (veja RemoverAsync em ClinicaRepository).
public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : Entity
{
    protected readonly ClinicaDbContext Contexto;

    protected RepositoryBase(ClinicaDbContext contexto)
    {
        Contexto = contexto;
    }

    // A tabela da entidade T. Para Clinica, é o mesmo que Contexto.Clinicas.
    protected DbSet<T> Tabela => Contexto.Set<T>();

    public virtual Task<List<T>> ListarAsync()
    {
        // AsNoTracking: é só leitura, o EF não precisa vigiar mudanças nesses objetos.
        return Tabela
            .AsNoTracking()
            .OrderBy(entidade => entidade.Id)
            .ToListAsync();
    }

    public virtual Task<T?> BuscarPorIdAsync(int id)
    {
        // Aqui NÃO tem AsNoTracking: a entidade devolvida é a que depois vai para
        // AtualizarAsync ou RemoverAsync, e o EF precisa estar acompanhando ela.
        return Tabela.FirstOrDefaultAsync(entidade => entidade.Id == id);
    }

    public virtual async Task AdicionarAsync(T entidade)
    {
        Tabela.Add(entidade);
        await Contexto.SaveChangesAsync();
    }

    public virtual async Task AtualizarAsync(T entidade)
    {
        Tabela.Update(entidade);
        await Contexto.SaveChangesAsync();
    }

    public virtual async Task RemoverAsync(T entidade)
    {
        Tabela.Remove(entidade);
        await Contexto.SaveChangesAsync();
    }
}
