using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> SearchAsync(int? rating, CancellationToken cancellationToken = default);
    Task<Review?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Review>> GetForBookAsync(int bookId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
}
