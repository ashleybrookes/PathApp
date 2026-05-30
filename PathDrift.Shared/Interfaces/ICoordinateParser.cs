using PathDrift.Shared.Models;

namespace PathDrift.Shared.Interfaces;

/// <summary>
/// Abstraction for parsing a raw text line into a Coordinate.
/// </summary>
public interface ICoordinateParser
{
    /// <summary>
    /// Attempts to parse a tab-separated line into a Coordinate.
    /// Returns null if the line is a header or malformed.
    /// </summary>
    Coordinate? Parse(string line);
}

