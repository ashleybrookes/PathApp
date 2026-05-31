using System.Globalization;
using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Models;

namespace PathDrift.Primary.Services;

/// <summary>
/// Parses comma-separated path drift lines into Coordinate objects.
/// Expected format: Path_ID,Index,X,Y,Z,Rx,Ry,Rz - this is the only format currently supported, and lines that have less than 8 fields will be ignored. 
/// The first line is expected to be a header and will be skipped. All numeric fields are parsed using invariant culture to ensure consistent decimal handling regardless of locale.
/// </summary>
public sealed class CoordinateParserService : ICoordinateParser
{
    private const char Separator = ',';
    private const int ExpectedMinColumns = 8;

    /// <inheritdoc />
    /// TODO: better error handling and logging for malformed lines
    public Coordinate? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var parts = line.Split(Separator);

        if (parts.Length < ExpectedMinColumns)
            return null;

        // Skip header row
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
            return null;

        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
            !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
            !double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var z) ||
            !double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var Rx) ||
            !double.TryParse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var Ry) ||
            !double.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out var Rz)
            )
            return null;

        return new Coordinate(parts[0].Trim(), index, x, y, z, Rx, Ry, Rz);
    }
}

