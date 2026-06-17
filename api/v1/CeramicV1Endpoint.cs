namespace CeramicApi.V1;

public static class CeramicV1Endpoint
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp",
        ".jpeg",
        ".jpg",
        ".png"
    };

    public static IEndpointRouteBuilder MapCeramicV1Endpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/ceramic/files", () =>
            {
                try
                {
                    var dataDirectory = GetServerImageDirectory();
                    var files = Array.Empty<object>();

                    if (Directory.Exists(dataDirectory))
                    {
                        files = Directory.EnumerateFiles(dataDirectory, "*", SearchOption.AllDirectories)
                            .Select(filePath => new FileInfo(filePath))
                            .Where(fileInfo => SupportedImageExtensions.Contains(fileInfo.Extension))
                            .OrderBy(fileInfo => fileInfo.FullName, StringComparer.OrdinalIgnoreCase)
                            .Select(fileInfo =>
                            {
                                var relativePath = Path.GetRelativePath(dataDirectory, fileInfo.FullName)
                                    .Replace(Path.DirectorySeparatorChar, '/');

                                return (object)new
                                {
                                    name = fileInfo.Name,
                                    relativePath,
                                    size = fileInfo.Length,
                                    url = $"/api/ceramic/server-image?relativePath={Uri.EscapeDataString(relativePath)}"
                                };
                            })
                            .ToArray();
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        folderPath = dataDirectory,
                        displayPath = GetServerImageDisplayPath(),
                        count = files.Length,
                        files
                    });
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    return Results.Problem(
                        title: "Failed to read ceramic image list",
                        detail: error.Message,
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/ceramic/server-image", (string relativePath) =>
            {
                try
                {
                    var imagePath = ResolveServerImagePath(relativePath);
                    return Results.File(
                        path: imagePath,
                        contentType: GetImageContentType(imagePath),
                        fileDownloadName: Path.GetFileName(imagePath),
                        enableRangeProcessing: true);
                }
                catch (ArgumentException error)
                {
                    return Results.Problem(
                        title: "Invalid ceramic image path",
                        detail: error.Message,
                        statusCode: StatusCodes.Status400BadRequest);
                }
            })
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapPost("/api/ceramic/v1", async (
                HttpRequest request,
                HttpResponse response,
                ICeramicV1Processor processor,
                IImageInputValidator validator,
                CancellationToken cancellationToken) =>
            await ImageRequestHandler.ProcessImageAsync(
                request,
                response,
                processor,
                validator,
                cancellationToken))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapPost("/api/ceramic/v1/server-file", async (
                string relativePath,
                HttpResponse response,
                ICeramicV1Processor processor,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var imagePath = ResolveServerImagePath(relativePath);
                    var imageBytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
                    var input = new CeramicImageInput(
                        FileName: Path.GetFileName(imagePath),
                        ContentType: GetImageContentType(imagePath),
                        Bytes: imageBytes);
                    var result = await processor.ProcessAsync(input, cancellationToken);

                    response.Headers["X-Ceramic-Version"] = processor.Version;

                    return Results.File(
                        fileContents: result.Bytes,
                        contentType: result.ContentType,
                        fileDownloadName: result.FileName);
                }
                catch (ArgumentException error)
                {
                    return Results.Problem(
                        title: "Invalid ceramic image path",
                        detail: error.Message,
                        statusCode: StatusCodes.Status400BadRequest);
                }
            })
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private static string GetServerImageDirectory()
    {
        return Path.GetFullPath(
            Environment.GetEnvironmentVariable("CERAMIC_XRAY_DATA_DIR") ??
            "/app/x-ray");
    }

    private static string GetServerImageDisplayPath()
    {
        return Environment.GetEnvironmentVariable("CERAMIC_XRAY_DATA_DISPLAY_PATH") ??
               "/home/nami/repo/gpt_analysis/project/Fault-Extraction-of-Ceramic-Images/x-ray";
    }

    private static string ResolveServerImagePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("relativePath is required.");
        }

        var dataDirectory = GetServerImageDirectory();
        var imagePath = Path.GetFullPath(Path.Combine(dataDirectory, relativePath));

        if (!imagePath.StartsWith(dataDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !string.Equals(imagePath, dataDirectory, StringComparison.Ordinal))
        {
            throw new ArgumentException("The requested image is outside the configured ceramic x-ray folder.");
        }

        if (!File.Exists(imagePath))
        {
            throw new ArgumentException("The requested ceramic image does not exist.");
        }

        if (!SupportedImageExtensions.Contains(Path.GetExtension(imagePath)))
        {
            throw new ArgumentException("Only BMP, JPG, JPEG, and PNG images are supported.");
        }

        return imagePath;
    }

    private static string GetImageContentType(string imagePath)
    {
        return Path.GetExtension(imagePath).ToLowerInvariant() switch
        {
            ".bmp" => "image/bmp",
            ".jpeg" => "image/jpeg",
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}
