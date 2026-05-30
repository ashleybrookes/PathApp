using Grpc.Core;
using Microsoft.Extensions.Logging;
using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Models;
using PathDrift.Shared.Protos;

namespace PathViewingApp.Services;

/// <summary>
/// gRPC server implementation that receives streamed coordinates from the primary service.
/// </summary>
public sealed class CoordinateReceiverService : CoordinateService.CoordinateServiceBase
{
    private readonly ICoordinateStore _store;
    private readonly ILogger<CoordinateReceiverService> _logger;

    public CoordinateReceiverService(ICoordinateStore store, ILogger<CoordinateReceiverService> logger)
    {
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Receives a stream of coordinates from the client and stores them.
    /// </summary>
    public override async Task<StreamResult> StreamCoordinates(
        IAsyncStreamReader<CoordinateMessage> requestStream,
        ServerCallContext context)
    {
        var count = 0;

        try
        {
            await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
            {
                var coordinate = new Coordinate(
                    message.PathId,
                    message.Index,
                    message.X,
                    message.Y,
                    message.Z);

                _store.Add(coordinate);
                count++;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Client stream was cancelled after {Count} points", count);
        }

        _logger.LogInformation("Received {Count} coordinate points", count);

        return new StreamResult
        {
            Success = true,
            PointsReceived = count,
            Message = $"Successfully received {count} coordinate points"
        };
    }
}

