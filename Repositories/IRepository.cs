namespace BibliotecaAspNet.Repositories;

/// <summary>
/// Operaciones CRUD comunes. En Spring Data el framework ofrece estas operaciones
/// mediante JpaRepository; aquí se ven explícitamente para facilitar la comparación.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
