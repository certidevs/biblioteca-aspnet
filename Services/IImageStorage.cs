using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

public interface IImageStorage
{
    Task<ImageUploadResult> SaveAsync(
        IFormFile file,
        ImageFolder folder,
        CancellationToken cancellationToken = default);

    void Delete(ImageFolder folder, string? fileName);

    string? GetUrl(ImageFolder folder, string? fileName);
}
