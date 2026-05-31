using PathDrift.Shared.Models;

namespace PathDrift.Shared.Tests;

public class CoordinateTests
{
    [Fact]
    public void Coordinate_RecordEquality_ShouldWorkByValue()
    {
        var a = new Coordinate("p1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0);
        var b = new Coordinate("p1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Coordinate_DifferentValues_ShouldNotBeEqual()
    {
        var a = new Coordinate("p1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0);
        var b = new Coordinate("p1", 1, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Coordinate_WithExpression_ShouldCreateModifiedCopy()
    {
        var original = new Coordinate("p1", 0, 1.0, 2.0, 3.0, 0.1, 0.2, 0.3);

        var modified = original with { X = 99.0 };

        Assert.Equal(99.0, modified.X);
        Assert.Equal(original.Y, modified.Y);
        Assert.Equal(original.PathId, modified.PathId);
    }

    [Fact]
    public void Coordinate_ToString_ShouldContainAllProperties()
    {
        var coordinate = new Coordinate("path1", 5, 1.1, 2.2, 3.3, 0.4, 0.5, 0.6);

        var result = coordinate.ToString();

        Assert.Contains("path1", result);
        Assert.Contains("1.1", result);
        Assert.Contains("2.2", result);
        Assert.Contains("3.3", result);
    }

    [Fact]
    public void Coordinate_Deconstruction_ShouldWork()
    {
        var coordinate = new Coordinate("p1", 0, 1.0, 2.0, 3.0, 0.1, 0.2, 0.3);

        var (pathId, index, x, y, z, rx, ry, rz) = coordinate;

        Assert.Equal("p1", pathId);
        Assert.Equal(0, index);
        Assert.Equal(1.0, x);
        Assert.Equal(2.0, y);
        Assert.Equal(3.0, z);
        Assert.Equal(0.1, rx);
        Assert.Equal(0.2, ry);
        Assert.Equal(0.3, rz);
    }
}
