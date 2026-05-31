using PathDrift.Shared.Models;
using PathViewingApp.Services;
using static PathViewingApp.Services.PathViewerLogic;

namespace PathViewingApp.Tests;

public class PathViewerLogicTests
{
    private readonly PathViewerLogic _logic = new();

    private static Coordinate MakeCoord(double x, double y, double z)
        => new("path1", 0, x, y, z, 0, 0, 0);

    #region GetH / GetV

    [Theory]
    [InlineData(Plane.XY, 1.0)]
    [InlineData(Plane.XZ, 1.0)]
    [InlineData(Plane.YZ, 2.0)]
    public void GetH_ShouldReturnCorrectAxis(Plane plane, double expected)
    {
        var c = MakeCoord(1.0, 2.0, 3.0);
        Assert.Equal(expected, _logic.GetH(c, plane));
    }

    [Theory]
    [InlineData(Plane.XY, 2.0)]
    [InlineData(Plane.XZ, 3.0)]
    [InlineData(Plane.YZ, 3.0)]
    public void GetV_ShouldReturnCorrectAxis(Plane plane, double expected)
    {
        var c = MakeCoord(1.0, 2.0, 3.0);
        Assert.Equal(expected, _logic.GetV(c, plane));
    }

    #endregion

    #region ComputeBounds

    [Fact]
    public void ComputeBounds_ShouldReturnMinMax()
    {
        var coords = new List<Coordinate>
        {
            MakeCoord(0, 0, 0),
            MakeCoord(10, 20, 30),
        };

        var (minH, maxH, minV, maxV) = _logic.ComputeBounds(coords, Plane.XY);

        Assert.Equal(0, minH);
        Assert.Equal(10, maxH);
        Assert.Equal(0, minV);
        Assert.Equal(20, maxV);
    }

    [Fact]
    public void ComputeBounds_IdenticalPoints_ShouldExpandRange()
    {
        var coords = new List<Coordinate> { MakeCoord(5, 5, 5) };

        var (minH, maxH, minV, maxV) = _logic.ComputeBounds(coords, Plane.XY);

        Assert.True(maxH > minH);
        Assert.True(maxV > minV);
    }

    #endregion

    #region ScalePoint

    [Fact]
    public void ScalePoint_ShouldReturnWithinSvgBounds()
    {
        var c = MakeCoord(5, 5, 0);

        var (x, y) = _logic.ScalePoint(c, Plane.XY, 0, 10, 0, 10);

        Assert.InRange(x, 0, PathViewerLogic.SvgWidth);
        Assert.InRange(y, 0, PathViewerLogic.SvgHeight);
    }

    [Fact]
    public void ScalePoint_AtMinBounds_ShouldBePaddingEdge()
    {
        var c = MakeCoord(0, 0, 0);

        var (x, y) = _logic.ScalePoint(c, Plane.XY, 0, 10, 0, 10);

        Assert.Equal(PathViewerLogic.Padding, x);
        Assert.Equal(PathViewerLogic.SvgHeight - PathViewerLogic.Padding, y);
    }

    [Fact]
    public void ScalePoint_AtMaxBounds_ShouldBeOppositeEdge()
    {
        var c = MakeCoord(10, 10, 0);

        var (x, y) = _logic.ScalePoint(c, Plane.XY, 0, 10, 0, 10);

        Assert.Equal(PathViewerLogic.SvgWidth - PathViewerLogic.Padding, x);
        Assert.Equal(PathViewerLogic.Padding, y);
    }

    #endregion

    #region GetPolylinePoints

    [Fact]
    public void GetPolylinePoints_ShouldReturnSpaceSeparatedPairs()
    {
        var coords = new List<Coordinate>
        {
            MakeCoord(0, 0, 0),
            MakeCoord(10, 10, 10),
        };

        var result = _logic.GetPolylinePoints(coords, Plane.XY, 0, 10, 0, 10);

        var parts = result.Split(' ');
        Assert.Equal(2, parts.Length);
        Assert.All(parts, p => Assert.Contains(",", p));
    }

    #endregion

    #region Axis Labels

    [Theory]
    [InlineData(Plane.XY, "X")]
    [InlineData(Plane.XZ, "X")]
    [InlineData(Plane.YZ, "Y")]
    public void GetHAxisLabel_ShouldReturnCorrectLabel(Plane plane, string expected)
    {
        Assert.Equal(expected, _logic.GetHAxisLabel(plane));
    }

    [Theory]
    [InlineData(Plane.XY, "Y")]
    [InlineData(Plane.XZ, "Z")]
    [InlineData(Plane.YZ, "Z")]
    public void GetVAxisLabel_ShouldReturnCorrectLabel(Plane plane, string expected)
    {
        Assert.Equal(expected, _logic.GetVAxisLabel(plane));
    }

    #endregion
}
