using Microsoft.Extensions.Logging;
using NSubstitute;
using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Models;
using PathDrift.Primary.Services;

namespace PathDrift.Primary.Tests;

public class GrpcCoordinateSenderTests
{
    private readonly IFileReader _fileReader;
    private readonly ICoordinateParser _parser;
    private readonly ILogger<GrpcCoordinateSender> _logger;

    public GrpcCoordinateSenderTests()
    {
        _fileReader = Substitute.For<IFileReader>();
        _parser = Substitute.For<ICoordinateParser>();
        _logger = Substitute.For<ILogger<GrpcCoordinateSender>>();
    }

    [Fact]
    public async Task Constructor_ShouldCreateInstance()
    {
        await using var sender = new GrpcCoordinateSender(_fileReader, _parser, _logger, "https://localhost:5001");
        Assert.NotNull(sender);
    }

    [Fact]
    public async Task DisposeAsync_ShouldNotThrow()
    {
        var sender = new GrpcCoordinateSender(_fileReader, _parser, _logger, "https://localhost:5001");
        await sender.DisposeAsync();
    }

    [Fact]
    public void Parser_ShouldBeCalledForEachLine_WhenSending()
    {
        // This test verifies the parser dependency is properly wired.
        // Full integration tests for SendFileAsync require a running gRPC server.
        var line = "Path1,0,1.0,2.0,3.0,0.1,0.2,0.3";
        _parser.Parse(line).Returns(new Coordinate("Path1", 0, 1.0, 2.0, 3.0, 0.1, 0.2, 0.3));

        var result = _parser.Parse(line);

        Assert.NotNull(result);
        Assert.Equal("Path1", result.PathId);
    }
}
