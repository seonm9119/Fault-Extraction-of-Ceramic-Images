namespace CeramicApi.V1;

public sealed class CeramicV1Processor : ICeramicV1Processor
{
    private readonly CeramicV1Processing processing;

    public CeramicV1Processor(CeramicV1Processing processing)
    {
        this.processing = processing;
    }

    public string Version => "ceramic-v1";

    public async Task<CeramicProcessResult> ProcessAsync(
        CeramicImageInput input,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resultBytes = await processing.RunAsync(input.Bytes, input.FileName, cancellationToken);

        return new CeramicProcessResult(
            Bytes: resultBytes,
            ContentType: "image/png",
            FileName: BuildResultFileName(input.FileName, "v1"));
    }

    private static string BuildResultFileName(string fileName, string suffix)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "result";
        }

        return $"{baseName}-{suffix}-result.png";
    }
}
