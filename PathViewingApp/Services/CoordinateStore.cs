using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Models;

namespace PathViewingApp.Services;

/// <summary>
/// Thread-safe in-memory store for received coordinates.
/// Notifies subscribers when new data is added.
/// </summary>
public sealed class CoordinateStore : ICoordinateStore
{
    private readonly Lock _lock = new();
    private readonly List<Coordinate> _coordinates = [];

    /// <inheritdoc />
    public IReadOnlyList<Coordinate> Coordinates
    {
        get
        {
            lock (_lock)
            {
                return _coordinates.ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc />
    public event Action? OnUpdated;

    /// <inheritdoc />
    public void Add(Coordinate coordinate)
    {
        lock (_lock)
        {
            _coordinates.Add(coordinate);
        }

        OnUpdated?.Invoke();
    }
}

