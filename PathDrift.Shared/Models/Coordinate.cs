namespace PathDrift.Shared.Models;

/// <summary>
/// Represents a single 3D coordinate point from the path drift data.
/// </summary>
/// TODO handle all the columns from the csv - ID	Index	X	Y	Z	Rx	Ry	Rz	Ux	Uy	Uz	UTranslation	URx	URy	URz
public sealed record Coordinate(string PathId, int Index, double X, double Y, double Z);

