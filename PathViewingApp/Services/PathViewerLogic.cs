using PathDrift.Shared.Models;

namespace PathViewingApp.Services;

/// <summary>
/// Contains all pure calculation logic for the path viewer, to be used in an svg image
/// separated from the Blazor component for testability.
/// </summary>
public sealed class PathViewerLogic
{
    //TODO: A nice to have would be to make these configurable in the UI
    public const int SvgWidth = 800;
    public const int SvgHeight = 600;
    public const int Padding = 40;

    public enum Plane { XY, XZ, YZ }

    /// <summary>
    /// Returns the horizontal axis value for a coordinate based on the active plane.
    /// </summary>
    public double GetH(Coordinate c, Plane plane) => plane switch
    {
        Plane.XY => c.X,
        Plane.XZ => c.X,
        Plane.YZ => c.Y,
        _ => c.X
    };

    /// <summary>
    /// Returns the vertical axis value for a coordinate based on the active plane.
    /// </summary>
    public double GetV(Coordinate c, Plane plane) => plane switch
    {
        Plane.XY => c.Y,
        Plane.XZ => c.Z,
        Plane.YZ => c.Z,
        _ => c.Y
    };

    /// <summary>
    /// Computes the min/max bounds for the given coordinates and plane.
    /// Prevents division by zero when all points are identical.
    /// </summary>
    public (double MinH, double MaxH, double MinV, double MaxV) ComputeBounds(
        IReadOnlyList<Coordinate> coordinates, Plane plane)
    {
        var hValues = coordinates.Select(c => GetH(c, plane)).ToList();
        var vValues = coordinates.Select(c => GetV(c, plane)).ToList();

        var minH = hValues.Min();
        var maxH = hValues.Max();
        var minV = vValues.Min();
        var maxV = vValues.Max();

        //Prevents division by zero when all points are identical.
        if (Math.Abs(maxH - minH) < 0.0001) { minH -= 1; maxH += 1; }
        if (Math.Abs(maxV - minV) < 0.0001) { minV -= 1; maxV += 1; }

        return (minH, maxH, minV, maxV);
    }

    /// <summary>
    /// Scales a coordinate to SVG pixel space.
    /// </summary>
    public (double X, double Y) ScalePoint(
        Coordinate c, Plane plane,
        double minH, double maxH, double minV, double maxV)
    {
        var h = GetH(c, plane);
        var v = GetV(c, plane);

        var x = Padding + (h - minH) / (maxH - minH) * (SvgWidth - 2 * Padding);
        // Invert Y axis for SVG (top is 0)
        var y = SvgHeight - Padding - (v - minV) / (maxV - minV) * (SvgHeight - 2 * Padding);

        return (Math.Round(x, 2), Math.Round(y, 2));
    }

    /// <summary>
    /// Builds the SVG polyline points string for all coordinates.
    /// </summary>
    public string GetPolylinePoints(
        IReadOnlyList<Coordinate> coordinates, Plane plane,
        double minH, double maxH, double minV, double maxV)
    {
        return string.Join(" ", coordinates.Select(c =>
        {
            var (x, y) = ScalePoint(c, plane, minH, maxH, minV, maxV);
            return $"{x},{y}";
        }));
    }

    /// <summary>
    /// Returns the horizontal axis label for the given plane.
    /// </summary>
    public string GetHAxisLabel(Plane plane) => plane switch
    {
        Plane.YZ => "Y",
        _ => "X"
    };

    /// <summary>
    /// Returns the vertical axis label for the given plane.
    /// </summary>
    public string GetVAxisLabel(Plane plane) => plane switch
    {
        Plane.XY => "Y",
        _ => "Z"
    };
}
