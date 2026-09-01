namespace BibliotecaAspNet.Services;

public sealed record StoredImage(string FileName, string ContentType);

public sealed record ImageUploadResult(StoredImage? Image, string? Error)
{
    public bool Succeeded => Image is not null;

    public static ImageUploadResult Success(StoredImage image) => new(image, null);

    public static ImageUploadResult Failure(string error) => new(null, error);
}
