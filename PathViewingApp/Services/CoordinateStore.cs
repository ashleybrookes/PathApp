using Microsoft.Extensions.Logging;
using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Models;

namespace PathViewingApp.Services;

/// <summary>
/// Thread-safe in-memory store for received coordinates.
/// Notifies subscribers when new data is added.
/// Coordinates are stored in a queue with a maximum capacity (10,000) to prevent unbounded memory growth. When the capacity is exceeded, the oldest coordinate is removed.
/// </summary>
public sealed class CoordinateStore(ILogger<CoordinateStore> logger) : ICoordinateStore
{
    private readonly Lock _lock = new();
    private readonly Queue<Coordinate> _coordinates = new();
    private const int MaxCapacity = 10_000;
    private IReadOnlyList<Coordinate> _snapshot = Array.Empty<Coordinate>();

    /// <inheritdoc />
    public IReadOnlyList<Coordinate> Coordinates => _snapshot;

    /// <inheritdoc />
    public event Action? OnUpdated;

    /// <inheritdoc />
    public void Add(Coordinate coordinate)
    {
        lock (_lock)
        {
            if (_coordinates.Count >= MaxCapacity)
                _coordinates.Dequeue();

            _coordinates.Enqueue(coordinate);
            _snapshot = _coordinates.ToArray();
        }

        if (OnUpdated is null) return;

        foreach (var handler in OnUpdated.GetInvocationList().Cast<Action>())
        {
            try
            {
                handler();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A subscriber to OnUpdated threw an exception");
            }
        }
    }
}

