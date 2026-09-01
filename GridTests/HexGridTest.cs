using NUnit.Framework;
using UnityEngine;

public class GridHexXZTests
{
    private GridHexXZ grid;

    [SetUp]
    public void Setup()
    {
        grid = new GridHexXZ(
            width: 5,
            height: 5,
            cellSize: 1f,
            originPosition: Vector3.zero
        );
    }

    // 1. Test: IsInsideGrid valid
    [Test]
    public void IsInsideGrid_ValidCoordinates_ReturnsTrue()
    {
        Assert.IsTrue(grid.IsInsideGrid(2, 3));
    }

    // 2. Test: IsInsideGrid invalid
    [Test]
    public void IsInsideGrid_InvalidCoordinates_ReturnsFalse()
    {
        Assert.IsFalse(grid.IsInsideGrid(-1, 0));
        Assert.IsFalse(grid.IsInsideGrid(grid.GetWidth(), 0));
        Assert.IsFalse(grid.IsInsideGrid(0, grid.GetHeight()));
    }

    // 3. Test: SetTileState + GetTileState
    [Test]
    public void SetTileState_ThenGetTileState_ReturnsCorrectState()
    {
        grid.SetTileState(1, 1, TileState.Blocked);
        Assert.AreEqual(TileState.Blocked, grid.GetTileState(1, 1));
    }

    // 4. Test: SetTileState out of bounds throws
    [Test]
    public void SetTileState_OutOfBounds_ThrowsException()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() =>
        {
            grid.SetTileState(99, 99, TileState.Free);
        });
    }

    // 5. Test: GetGridObject returns non-null
    [Test]
    public void GetGridObject_ValidCoordinates_ReturnsObject()
    {
        GridObject obj = grid.GetGridObject(2, 2);
        Assert.IsNotNull(obj);
    }

    // 6. Test: GetGridObject out of bounds throws
    [Test]
    public void GetGridObject_OutOfBounds_ThrowsException()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() =>
        {
            grid.GetGridObject(-1, -1);
        });
    }

    // 7. Test: GetWorldPosition returns correct offset
    [Test]
    public void GetWorldPosition_ReturnsCorrectPosition()
    {
        Vector3 pos = grid.GetWorldPosition(2, 0);
        Assert.AreEqual(new Vector3(2f, 0f, 0f), pos);
    }
}
