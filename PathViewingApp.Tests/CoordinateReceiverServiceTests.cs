using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Models;
using PathDrift.Shared.Protos;
using PathViewingApp.Services;

namespace PathViewingApp.Tests;

public class CoordinateReceiverServiceTests
{
    private readonly ICoordinateStore _store;
    private readonly CoordinateReceiverService _service;

    public CoordinateReceiverServiceTests()
    {
        _store = Substitute.For<ICoordinateStore>();
        var logger = Substitute.For<ILogger<CoordinateReceiverService>>();
        _service = new CoordinateReceiverService(_store, logger);
    }

    [Fact]
    public async Task StreamCoordinates_WithMessages_ShouldStoreAll()
    {
        var messages = new List<CoordinateMessage>
        {
            new() { PathId = "p1", Index = 0, X = 1, Y = 2, Z = 3, Rx = 0, Ry = 0, Rz = 0 },
            new() { PathId = "p1", Index = 1, X = 4, Y = 5, Z = 6, Rx = 0, Ry = 0, Rz = 0 },
        };

        var reader = new TestAsyncStreamReader<CoordinateMessage>(messages);
        var context = TestServerCallContext.Create();

        var result = await _service.StreamCoordinates(reader, context);

        Assert.True(result.Success);
        Assert.Equal(2, result.PointsReceived);
        _store.Received(2).Add(Arg.Any<Coordinate>());
    }

    [Fact]
    public async Task StreamCoordinates_EmptyStream_ShouldReturnSuccessWithZeroPoints()
    {
        var reader = new TestAsyncStreamReader<CoordinateMessage>([]);
        var context = TestServerCallContext.Create();

        var result = await _service.StreamCoordinates(reader, context);

        Assert.True(result.Success);
        Assert.Equal(0, result.PointsReceived);
    }

    [Fact]
    public async Task StreamCoordinates_WhenCancelled_ShouldReturnFailure()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var reader = new TestAsyncStreamReader<CoordinateMessage>(
            [new() { PathId = "p1", Index = 0, X = 1, Y = 2, Z = 3, Rx = 0, Ry = 0, Rz = 0 }]);
        var context = TestServerCallContext.Create(cts.Token);

        var result = await _service.StreamCoordinates(reader, context);

        Assert.False(result.Success);
        Assert.Contains("cancelled", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Test helper that simulates an IAsyncStreamReader from a list.
/// </summary>
internal class TestAsyncStreamReader<T>(IList<T> messages) : IAsyncStreamReader<T>
{
    private int _index = -1;

    public T Current => messages[_index];

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        _index++;
        return _index < messages.Count;
    }
}

/// <summary>
/// Minimal test implementation of ServerCallContext.
/// </summary>
internal class TestServerCallContext : ServerCallContext
{
    private readonly CancellationToken _cancellationToken;

    private TestServerCallContext(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    public static ServerCallContext Create(CancellationToken cancellationToken = default)
        => new TestServerCallContext(cancellationToken);

    protected override string MethodCore => "TestMethod";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "test-peer";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore => [];
    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override Metadata ResponseTrailersCore => [];
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => new(null, []);

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => throw new NotImplementedException();
    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
}
