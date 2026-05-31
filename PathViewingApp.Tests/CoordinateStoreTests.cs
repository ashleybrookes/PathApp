using Microsoft.Extensions.Logging;
using NSubstitute;
using PathDrift.Shared.Models;
using PathViewingApp.Services;

namespace PathViewingApp.Tests;

public class CoordinateStoreTests
{
    private readonly CoordinateStore _store;

    public CoordinateStoreTests()
    {
        var logger = Substitute.For<ILogger<CoordinateStore>>();
        _store = new CoordinateStore(logger);
    }

    [Fact]
    public void Add_ShouldStoreCoordinate()
    {
        var coordinate = new Coordinate("path1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0);

        _store.Add(coordinate);

        Assert.Single(_store.Coordinates);
        Assert.Equal(coordinate, _store.Coordinates[0]);
    }

    [Fact]
    public void Add_MultipleCoordinates_ShouldStoreAll()
    {
        var c1 = new Coordinate("path1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0);
        var c2 = new Coordinate("path1", 1, 4.0, 5.0, 6.0, 0.0, 0.0, 0.0);

        _store.Add(c1);
        _store.Add(c2);

        Assert.Equal(2, _store.Coordinates.Count);
    }

    [Fact]
    public void Add_ShouldRaiseOnUpdatedEvent()
    {
        var raised = false;
        _store.OnUpdated += () => raised = true;

        _store.Add(new Coordinate("path1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0));

        Assert.True(raised);
    }

    [Fact]
    public void Add_WhenSubscriberThrows_ShouldNotPreventStorage()
    {
        _store.OnUpdated += () => throw new InvalidOperationException("Test exception");

        _store.Add(new Coordinate("path1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0));

        Assert.Single(_store.Coordinates);
    }

    [Fact]
    public void Coordinates_ShouldReturnSnapshot()
    {
        _store.Add(new Coordinate("path1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0));

        var snapshot = _store.Coordinates;

        _store.Add(new Coordinate("path1", 1, 4.0, 5.0, 6.0, 0.0, 0.0, 0.0));

        Assert.Single(snapshot);
        Assert.Equal(2, _store.Coordinates.Count);
    }
}
