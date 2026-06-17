namespace CeramicApi.V1;

public interface IImageInputValidator
{
    string? Validate(IFormFile file);
}

public sealed class ImageInputValidator : IImageInputValidator
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/bmp",
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp",
        ".jpeg",
        ".jpg",
        ".png"
    };

    public string? Validate(IFormFile file)
    {
        if (file.Length <= 0)
        {
            return "The uploaded file is empty.";
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return "Only BMP, JPG, JPEG, and PNG images are supported.";
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType) &&
            !AllowedContentTypes.Contains(file.ContentType))
        {
            return $"Unsupported content type: {file.ContentType}.";
        }

        return null;
    }
}

public static class ImageRequestHandler
{
    public static async Task<IResult> ProcessImageAsync(
        HttpRequest request,
        HttpResponse response,
        ICeramicProcessor processor,
        IImageInputValidator validator,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.Problem(
                title: "Invalid content type",
                detail: "Use multipart/form-data and send the image in the 'image' field.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("image") ?? form.Files.FirstOrDefault();

        if (file is null)
        {
            return Results.Problem(
                title: "Image file is required",
                detail: "Attach an image file using the 'image' form field.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var validationError = validator.Validate(file);
        if (validationError is not null)
        {
            return Results.Problem(
                title: "Invalid image file",
                detail: validationError,
                statusCode: StatusCodes.Status400BadRequest);
        }

        await using var inputStream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream, cancellationToken);

        var input = new CeramicImageInput(
            FileName: Path.GetFileName(file.FileName),
            ContentType: file.ContentType,
            Bytes: memoryStream.ToArray());

        var result = await processor.ProcessAsync(input, cancellationToken);

        response.Headers["X-Ceramic-Version"] = processor.Version;

        return Results.File(
            fileContents: result.Bytes,
            contentType: result.ContentType,
            fileDownloadName: result.FileName);
    }
}
