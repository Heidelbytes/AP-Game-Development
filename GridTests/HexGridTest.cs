using NUnit.Framework;
using UnityEngine;

public class HexGridTests
{
    private HexGrid grid;

    [SetUp]
    public void Setup()
    {
        grid = new HexGrid(
            width: 10,
            height: 10,
            cellSize: 1f,
            originPosition: Vector3.zero
        );
    }

    // --- GRID INFO ---

    [Test]
    public void GridWidth_IsCorrect()
    {
        Assert.AreEqual(10, grid.GetGridWidth());
    }

    [Test]
    public void GridHeight_IsCorrect()
    {
        Assert.AreEqual(10, grid.GetGridHeight());
    }

    [Test]
    public void IsInsideGrid_ValidCoordinates_ReturnsTrue()
    {
        Assert.IsTrue(grid.IsInsideGrid(5, 5));
    }

    [Test]
    public void IsInsideGrid_InvalidCoordinates_ReturnsFalse()
    {
        Assert.IsFalse(grid.IsInsideGrid(-1, 0));
        Assert.IsFalse(grid.IsInsideGrid(0, -1));
        Assert.IsFalse(grid.IsInsideGrid(10, 0));
        Assert.IsFalse(grid.IsInsideGrid(0, 10));
    }

    // --- WORLD POSITION / COORDINATES ---

    [Test]
    public void GetHexTileWorldPosition_OddRowHasOffset()
    {
        Vector3 evenRow = grid.GetHexTileWorldPosition(2, 2);
        Vector3 oddRow = grid.GetHexTileWorldPosition(2, 3);

        Assert.AreEqual(evenRow.x + 0.5f, oddRow.x, 0.001f);
    }

    [Test]
    public void WorldToHexTile_ReturnsCorrectCoordinates()
    {
        Vector3 worldPos = grid.GetHexTileWorldPosition(4, 6);

        grid.WorldToHexTile(worldPos, out int x, out int z);

        Assert.AreEqual(4, x);
        Assert.AreEqual(6, z);
    }

    // --- DISTANCE ---

    [Test]
    public void GetHexTileDistance_CorrectCubeDistance()
    {
        int dist = grid.GetHexTileDistance(0, 0, 3, 3);

        Assert.AreEqual(3, dist);
    }

    // --- TILE STATE ---

    [Test]
    public void SetHexTileState_StoresCorrectState()
    {
        grid.SetHexTileState(2, 2, TileState.Blocked);

        Assert.AreEqual(TileState.Blocked, grid.GetHexTileState(2, 2));
    }

    [Test]
    public void GetHexTile_OutOfBounds_ThrowsException()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() =>
        {
            grid.GetHexTile(99, 99);
        });
    }
}
