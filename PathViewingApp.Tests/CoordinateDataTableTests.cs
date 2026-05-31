using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PathDrift.Shared.Interfaces;
using PathDrift.Shared.Models;
using PathViewingApp.Components;

namespace PathViewingApp.Tests;

public class CoordinateDataTableTests : TestContext
{
    private readonly ICoordinateStore _store;

    public CoordinateDataTableTests()
    {
        _store = Substitute.For<ICoordinateStore>();
        _store.Coordinates.Returns(new List<Coordinate>().AsReadOnly());
        Services.AddSingleton<ICoordinateStore>(_store);
    }

    [Fact]
    public void WhenNoData_ShouldShowInfoMessage()
    {
        var cut = Render<CoordinateDataTable>();

        cut.Find(".alert-info").MarkupMatches(
            "<div class=\"alert alert-info\">No stream data received yet.</div>");
    }

    [Fact]
    public void WhenDataExists_ShouldRenderTableRows()
    {
        var coords = new List<Coordinate>
        {
            new("p1", 0, 1.0, 2.0, 3.0, 0.1, 0.2, 0.3)
        };
        _store.Coordinates.Returns(coords.AsReadOnly());

        var cut = Render<CoordinateDataTable>();

        var rows = cut.FindAll("tbody tr");
        Assert.Single(rows);
        Assert.Contains("p1", rows[0].InnerHtml);
    }

    [Fact]
    public void WhenDataExists_ShouldShowPointCount()
    {
        var coords = new List<Coordinate>
        {
            new("p1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0),
            new("p1", 1, 4.0, 5.0, 6.0, 0.0, 0.0, 0.0),
        };
        _store.Coordinates.Returns(coords.AsReadOnly());

        var cut = Render<CoordinateDataTable>();

        var badge = cut.Find(".badge");
        Assert.Contains("2 points", badge.TextContent);
    }

    [Fact]
    public void WhenDataExists_ShouldFormatNumbersCorrectly()
    {
        var coords = new List<Coordinate>
        {
            new("p1", 0, 1.1234567, 2.1234567, 3.1234567, 0.123456789, 0.223456789, 0.323456789)
        };
        _store.Coordinates.Returns(coords.AsReadOnly());

        var cut = Render<CoordinateDataTable>();

        var row = cut.Find("tbody tr");
        // X, Y, Z should be formatted to 7 decimal places
        Assert.Contains("1.1234567", row.InnerHtml);
        // Rx, Ry, Rz should be formatted to 9 decimal places
        Assert.Contains("0.123456789", row.InnerHtml);
    }

    [Fact]
    public void WhenStoreUpdates_ShouldRefreshData()
    {
        var coords = new List<Coordinate>();
        _store.Coordinates.Returns(coords.AsReadOnly());

        var cut = Render<CoordinateDataTable>();
        Assert.Empty(cut.FindAll("tbody tr"));

        // Simulate new data arriving
        var updatedCoords = new List<Coordinate>
        {
            new("p1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0)
        };
        _store.Coordinates.Returns(updatedCoords.AsReadOnly());
        _store.OnUpdated += Raise.Event<Action>();

        Assert.Single(cut.FindAll("tbody tr"));
    }

    [Fact]
    public void WhenNoData_ShouldNotRenderTable()
    {
        var cut = Render<CoordinateDataTable>();

        Assert.Empty(cut.FindAll("table"));
    }

    [Fact]
    public void Table_ShouldHaveCorrectHeaders()
    {
        var coords = new List<Coordinate>
        {
            new("p1", 0, 1.0, 2.0, 3.0, 0.0, 0.0, 0.0)
        };
        _store.Coordinates.Returns(coords.AsReadOnly());

        var cut = Render<CoordinateDataTable>();

        var headers = cut.FindAll("thead th");
        Assert.Equal(8, headers.Count);
        Assert.Equal("Path ID", headers[0].TextContent);
        Assert.Equal("Index", headers[1].TextContent);
        Assert.Equal("X", headers[2].TextContent);
        Assert.Equal("Y", headers[3].TextContent);
        Assert.Equal("Z", headers[4].TextContent);
        Assert.Equal("Rx", headers[5].TextContent);
        Assert.Equal("Ry", headers[6].TextContent);
        Assert.Equal("Rz", headers[7].TextContent);
    }
}
