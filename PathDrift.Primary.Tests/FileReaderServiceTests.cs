using PathDrift.Primary.Services;

namespace PathDrift.Primary.Tests;

public class FileReaderServiceTests
{
    private readonly FileReaderService _reader = new();

    [Fact]
    public async Task ReadLinesAsync_ValidFile_ShouldReturnAllLines()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllLinesAsync(tempFile, ["line1", "line2", "line3"]);

            var lines = new List<string>();
            await foreach (var line in _reader.ReadLinesAsync(tempFile))
            {
                lines.Add(line);
            }

            Assert.Equal(3, lines.Count);
            Assert.Equal("line1", lines[0]);
            Assert.Equal("line2", lines[1]);
            Assert.Equal("line3", lines[2]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadLinesAsync_EmptyFile_ShouldReturnNoLines()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var lines = new List<string>();
            await foreach (var line in _reader.ReadLinesAsync(tempFile))
            {
                lines.Add(line);
            }

            Assert.Empty(lines);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadLinesAsync_FileNotFound_ShouldThrowFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");

        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await foreach (var _ in _reader.ReadLinesAsync(nonExistentPath))
            {
                // Should not reach here
            }
        });
    }

    [Fact]
    public async Task ReadLinesAsync_CancellationRequested_ShouldStop()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllLinesAsync(tempFile, ["line1", "line2", "line3", "line4", "line5"]);

            var cts = new CancellationTokenSource();
            var lines = new List<string>();

            await foreach (var line in _reader.ReadLinesAsync(tempFile, cts.Token))
            {
                lines.Add(line);
                if (lines.Count == 2)
                    cts.Cancel();
            }

            // Service exits gracefully on cancellation without reading all lines
            Assert.True(lines.Count < 5, $"Expected fewer than 5 lines but got {lines.Count}");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
