using PathDrift.Shared.Interfaces;

namespace PathDrift.Primary.Services;

/// <summary>
/// Reads lines from a file asynchronously, one line at a time for memory efficiency.
/// </summary>
public sealed class FileReaderService : IFileReader
{
    /// <inheritdoc />
    public async IAsyncEnumerable<string> ReadLinesAsync(
        string filePath,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Path drift file not found: {filePath}", filePath);

        using var reader = new StreamReader(filePath);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
                break;
            yield return line;
        }
    }
}

