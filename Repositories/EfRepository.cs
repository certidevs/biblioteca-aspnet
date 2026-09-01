using BibliotecaAspNet.Data;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

/// <summary>
/// Implementación CRUD genérica basada en <see cref="ApplicationDbContext"/>.
/// Las consultas especiales viven en repositorios concretos.
/// </summary>
public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    /// <summary>Contexto compartido por el repositorio durante la petición HTTP.</summary>
    protected ApplicationDbContext Context { get; }

    /// <summary>Recibe el contexto mediante inyección de dependencias.</summary>
    public EfRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    /// <summary>Busca por clave primaria usando el mecanismo genérico de EF Core.</summary>
    public virtual async Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync(
            new object?[] { id },
            cancellationToken);
    }

    /// <summary>Lee todas las entidades sin seguimiento porque no se van a editar.</summary>
    public virtual Task<List<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return Context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>Añade una entidad al contexto; el INSERT ocurre al guardar cambios.</summary>
    public virtual Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        return Context.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    /// <summary>Marca la entidad como modificada para que EF genere un UPDATE.</summary>
    public virtual void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    /// <summary>Marca la entidad para borrado; todavía no ejecuta el DELETE.</summary>
    public virtual void Delete(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    /// <summary>Persiste las operaciones pendientes en una unidad de trabajo.</summary>
    public virtual Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}
