using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

public interface IReviewService
{
    Task<List<Review>> SearchAsync(int? rating, CancellationToken cancellationToken = default);
    Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(Review review, string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, Review review, string userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken = default);
    bool CanModify(Review review, string userId, bool isAdmin);
}
