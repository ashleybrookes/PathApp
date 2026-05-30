using PathDrift.Shared.Models;

namespace PathDrift.Shared.Interfaces;

/// <summary>
/// Thread-safe store for received coordinates. Raises an event when new data arrives.
/// </summary>
public interface ICoordinateStore
{
    /// <summary>
    /// All coordinates received so far.
    /// </summary>
    IReadOnlyList<Coordinate> Coordinates { get; }

    /// <summary>
    /// Adds a coordinate to the store and raises the OnUpdated event.
    /// </summary>
    void Add(Coordinate coordinate);

    /// <summary>
    /// Event raised when new coordinates are added.
    /// </summary>
    event Action? OnUpdated;
}

