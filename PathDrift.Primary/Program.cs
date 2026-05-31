using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PathDrift.Primary.Services;
using PathDrift.Shared.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<IFileReader, FileReaderService>();
builder.Services.AddSingleton<ICoordinateParser, CoordinateParserService>();

using var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var filePathConfig = config["PathDrift:FilePath"]
    ?? throw new InvalidOperationException("PathDrift:FilePath is not configured.");
var filePath = Path.IsPathRooted(filePathConfig)
    ? filePathConfig
    : Path.Join(AppContext.BaseDirectory, filePathConfig);
var serverAddress = config["PathDrift:ServerAddress"]
    ?? throw new InvalidOperationException("PathDrift:ServerAddress is not configured.");

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var fileReader = host.Services.GetRequiredService<IFileReader>();
var parser = host.Services.GetRequiredService<ICoordinateParser>();

logger.LogInformation("Path Drift Primary Service starting");
logger.LogInformation("File: {FilePath}", filePath);
logger.LogInformation("Server: {ServerAddress}", serverAddress);

try
{
    await using var sender = new GrpcCoordinateSender(fileReader, parser,
        host.Services.GetRequiredService<ILogger<GrpcCoordinateSender>>(), serverAddress);

    var result = await sender.SendFileAsync(filePath);

    logger.LogInformation("Result — Success: {Success}, Points received: {Count}, Message: {Msg}",
        result.Success, result.PointsReceived, result.Message);
}
catch (FileNotFoundException ex)
{
    logger.LogError("File not found: {Message}", ex.Message);
    Environment.ExitCode = 1;
}
catch (Grpc.Core.RpcException ex)
{
    logger.LogError("gRPC error: {Status} — {Message}", ex.StatusCode, ex.Message);
    Environment.ExitCode = 2;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Unexpected error");
    Environment.ExitCode = 3;
}
