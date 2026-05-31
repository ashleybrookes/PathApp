using PathDrift.Primary.Services;

namespace PathDrift.Primary.Tests;

public class CoordinateParserServiceTests
{
    private readonly CoordinateParserService _parser = new();

    [Fact]
    public void Parse_ValidLine_ShouldReturnCoordinate()
    {
        var line = "Path1,0,1.0,2.0,3.0,0.1,0.2,0.3";

        var result = _parser.Parse(line);

        Assert.NotNull(result);
        Assert.Equal("Path1", result.PathId);
        Assert.Equal(0, result.Index);
        Assert.Equal(1.0, result.X);
        Assert.Equal(2.0, result.Y);
        Assert.Equal(3.0, result.Z);
        Assert.Equal(0.1, result.Rx);
        Assert.Equal(0.2, result.Ry);
        Assert.Equal(0.3, result.Rz);
    }

    [Fact]
    public void Parse_NullOrEmpty_ShouldReturnNull()
    {
        Assert.Null(_parser.Parse(null!));
        Assert.Null(_parser.Parse(""));
        Assert.Null(_parser.Parse("   "));
    }

    [Fact]
    public void Parse_TooFewColumns_ShouldReturnNull()
    {
        var line = "Path1,0,1.0,2.0";

        Assert.Null(_parser.Parse(line));
    }

    [Fact]
    public void Parse_HeaderRow_ShouldReturnNull()
    {
        var line = "PathId,Index,X,Y,Z,Rx,Ry,Rz";

        Assert.Null(_parser.Parse(line));
    }

    [Fact]
    public void Parse_InvalidNumericValues_ShouldReturnNull()
    {
        var line = "Path1,0,abc,2.0,3.0,0.1,0.2,0.3";

        Assert.Null(_parser.Parse(line));
    }

    [Fact]
    public void Parse_NegativeValues_ShouldParse()
    {
        var line = "Path1,5,-1.5,-2.5,-3.5,-0.1,-0.2,-0.3";

        var result = _parser.Parse(line);

        Assert.NotNull(result);
        Assert.Equal(5, result.Index);
        Assert.Equal(-1.5, result.X);
        Assert.Equal(-2.5, result.Y);
        Assert.Equal(-3.5, result.Z);
    }

    [Fact]
    public void Parse_ExtraColumns_ShouldStillParse()
    {
        var line = "Path1,0,1.0,2.0,3.0,0.1,0.2,0.3,extra,data";

        var result = _parser.Parse(line);

        Assert.NotNull(result);
        Assert.Equal("Path1", result.PathId);
    }

    [Fact]
    public void Parse_WhitespaceInPathId_ShouldTrim()
    {
        var line = " Path1 ,0,1.0,2.0,3.0,0.1,0.2,0.3";

        var result = _parser.Parse(line);

        Assert.NotNull(result);
        Assert.Equal("Path1", result.PathId);
    }
}
