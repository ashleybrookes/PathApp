using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Protos;

namespace PathDrift.Primary.Services;

/// <summary>
/// Reads coordinates from a file and streams them to the secondary service via gRPC.
/// </summary>
public sealed class GrpcCoordinateSender : IAsyncDisposable
{
    private readonly IFileReader _fileReader;
    private readonly ICoordinateParser _parser;
    private readonly ILogger<GrpcCoordinateSender> _logger;
    private readonly GrpcChannel _channel;
    private readonly CoordinateService.CoordinateServiceClient _client;

    public GrpcCoordinateSender(
        IFileReader fileReader,
        ICoordinateParser parser,
        ILogger<GrpcCoordinateSender> logger,
        string serverAddress)
    {
        _fileReader = fileReader;
        _parser = parser;
        _logger = logger;
        _channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    // Accept the .NET dev certificate (development only)
                    RemoteCertificateValidationCallback = (_, _, _, _) => true
                }
            }
        });
        _client = new CoordinateService.CoordinateServiceClient(_channel);
    }

    /// <summary>
    /// Reads the specified file and streams all parsed coordinates to the gRPC server.
    /// </summary>
    public async Task<StreamResult> SendFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting to stream coordinates from {FilePath}", filePath);

        using var call = _client.StreamCoordinates(cancellationToken: cancellationToken);
        var sentCount = 0;
        var skippedCount = 0;

        await foreach (var line in _fileReader.ReadLinesAsync(filePath, cancellationToken).ConfigureAwait(false))
        {
            var coordinate = _parser.Parse(line);

            if (coordinate is null)
            {
                skippedCount++;
                continue;
            }

            var message = new CoordinateMessage
            {
                PathId = coordinate.PathId,
                Index = coordinate.Index,
                X = coordinate.X,
                Y = coordinate.Y,
                Z = coordinate.Z,
                Rx = coordinate.Rx,
                Ry = coordinate.Ry,
                Rz = coordinate.Rz  
            };

            await call.RequestStream.WriteAsync(message, cancellationToken).ConfigureAwait(false);
            sentCount++;
        }

        // Signal that we're done sending
        await call.RequestStream.CompleteAsync().ConfigureAwait(false);

        var result = await call.ResponseAsync.ConfigureAwait(false);

        _logger.LogInformation(
            "Streaming complete. Sent: {Sent}, Skipped: {Skipped}. Server response: {Message}",
            sentCount, skippedCount, result.Message);

        return result;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _channel.Dispose();
        await ValueTask.CompletedTask;
    }
}

