namespace BibliotecaAspNet.Repositories;

/// <summary>
/// Operaciones CRUD comunes. En Spring Data el framework ofrece estas operaciones
/// mediante JpaRepository; aquí se ven explícitamente para facilitar la comparación.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>Busca una entidad por su clave primaria o devuelve <c>null</c>.</summary>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Devuelve todas las entidades sin rastrearlas para una lectura más ligera.</summary>
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Marca una entidad nueva para insertarla al guardar cambios.</summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Marca una entidad existente como modificada.</summary>
    void Update(TEntity entity);

    /// <summary>Marca una entidad para eliminarla al guardar cambios.</summary>
    void Delete(TEntity entity);

    /// <summary>Confirma en la base de datos las operaciones pendientes y devuelve cuántas filas afectó.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
