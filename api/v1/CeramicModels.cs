namespace CeramicApi.V1;

public sealed record CeramicImageInput(
    string FileName,
    string ContentType,
    byte[] Bytes);

public sealed record CeramicProcessResult(
    byte[] Bytes,
    string ContentType,
    string FileName);

public interface ICeramicProcessor
{
    string Version { get; }

    Task<CeramicProcessResult> ProcessAsync(
        CeramicImageInput input,
        CancellationToken cancellationToken);
}

public interface ICeramicV1Processor : ICeramicProcessor
{
}
