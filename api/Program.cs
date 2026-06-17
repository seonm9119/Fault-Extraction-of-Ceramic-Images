using CeramicApi.V1;
using Microsoft.AspNetCore.Http.Features;

const long maxUploadBytes = 50L * 1024L * 1024L;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadBytes;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadBytes;
});

builder.Services.AddSingleton<IImageInputValidator, ImageInputValidator>();
builder.Services.AddScoped<CeramicV1Processing>();
builder.Services.AddScoped<ICeramicV1Processor, CeramicV1Processor>();

var app = builder.Build();

app.MapCeramicV1Endpoint();

app.Run();
