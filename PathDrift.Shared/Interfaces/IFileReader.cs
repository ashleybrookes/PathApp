namespace PathDrift.Shared.Interfaces;

/// <summary>
/// Abstraction for reading lines from a file asynchronously.
/// </summary>
public interface IFileReader
{
    /// <summary>
    /// Reads all lines from the specified file path as an async enumerable.
    /// </summary>
    IAsyncEnumerable<string> ReadLinesAsync(string filePath, CancellationToken cancellationToken = default);
}

