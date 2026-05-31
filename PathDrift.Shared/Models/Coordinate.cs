namespace PathDrift.Shared.Models;

/// <summary>
/// Represents a single 3D coordinate point from the path drift data.
/// </summary>
public sealed record Coordinate(string PathId, int Index, double X, double Y, double Z, double Rx, double Ry, double Rz);

