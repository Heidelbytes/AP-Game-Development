using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/*
 * Requirement:
 * REQ-000 TileState Management 
 

 TODO: placement and removing of building tests 
*/


public class TileStateManagementTests
{
    private GameObject managerGO;
    private HexGridManager manager;

    private Material freeMat;
    private Material occMat;
    private Material blockMat;
    private Material permBlockMat;

    private Transform pfHexagon;
    private Transform gridParent;

    [SetUp]
    public void Setup()
    {
        managerGO = new GameObject("HexGridManager");
        managerGO.SetActive(false);

        manager = managerGO.AddComponent<HexGridManager>();

        freeMat = new Material(Shader.Find("Standard"));
        occMat = new Material(Shader.Find("Standard"));
        blockMat = new Material(Shader.Find("Standard"));
        permBlockMat = new Material(Shader.Find("Standard"));

        GameObject hexPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pfHexagon = hexPrefab.transform;

        GameObject parentGO = new GameObject("GridParent");
        gridParent = parentGO.transform;

        typeof(HexGridManager)
            .GetField("pfHexagon",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(manager, pfHexagon);

        typeof(HexGridManager)
            .GetField("gridParent",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(manager, gridParent);

        typeof(HexGridManager)
            .GetField("freeMaterial",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(manager, freeMat);

        typeof(HexGridManager)
            .GetField("occupiedMaterial",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(manager, occMat);

        typeof(HexGridManager)
            .GetField("blockedMaterial",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(manager, blockMat);

        typeof(HexGridManager)
            .GetField("permanentBlockedMaterial",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(manager, permBlockMat);

        managerGO.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(managerGO);
        Object.Destroy(pfHexagon.gameObject);
        Object.Destroy(gridParent.gameObject);

        Object.Destroy(freeMat);
        Object.Destroy(occMat);
        Object.Destroy(blockMat);
        Object.Destroy(permBlockMat);
    }

    //---------------------------------
    // INITIAL TILE STATE GENERATION
    //---------------------------------

    [UnityTest]
    public IEnumerator BeaconTiles_AreInitializedAsOccupied()
    {
        yield return null;

        Vector2Int center = manager.Grid.GetGridCenter();

        Assert.AreEqual(
            TileState.Occupied,
            manager.Grid.GetHexTileState(center.x, center.y)
        );
    }

    [UnityTest]
    public IEnumerator TilesInsideInitialBuildRadius_AreFree()
    {
        yield return null;

        Vector2Int center = manager.Grid.GetGridCenter();

        Assert.AreEqual(
            TileState.Free,
            manager.Grid.GetHexTileState(center.x + 2, center.y)
        );
    }

    [UnityTest]
    public IEnumerator RemainingTilesInsideMaximumBuildRadius_AreBlocked()
    {
        yield return null;

        Vector2Int center = manager.Grid.GetGridCenter();

        Assert.AreEqual(
            TileState.Blocked,
            manager.Grid.GetHexTileState(center.x + 5, center.y)
        );
    }

    [UnityTest]
    public IEnumerator TilesOutsideMaximumBuildRadius_ArePermanentBlocked()
    {
        yield return null;

        Vector2Int center = manager.Grid.GetGridCenter();

        Assert.AreEqual(
            TileState.PermanentBlocked,
            manager.Grid.GetHexTileState(center.x + 7, center.y)
        );
    }


    //---------------------------------
    // BUILDING PLACEMENT BEHAVIOUR
    //---------------------------------

    [Test]
    public void PlacingBuilding_SetsTileStateToOccupied()
    {
        Assert.Inconclusive(
            "Building placement not implemented yet."
        );
    }

    [Test]
    public void OccupiedTile_CannotBecomeOccupiedAgain()
    {
        Assert.Inconclusive(
            "Building placement not implemented yet."
        );
    }

    [Test]
    public void RemovingBuilding_ResetsTileStateToFree()
    {
        Assert.Inconclusive(
            "Building removal not implemented yet."
        );
    }

    [UnityTest]
    public IEnumerator BlockedTiles_UseBlockedState()
    {
        yield return null;

        Vector2Int center = manager.Grid.GetGridCenter();

        Assert.AreEqual(
            TileState.Blocked,
            manager.Grid.GetHexTileState(center.x + 5, center.y)
        );
    }

    [UnityTest]
    public IEnumerator PermanentBlockedTiles_UsePermanentBlockedState()
    {
        yield return null;

        Vector2Int center = manager.Grid.GetGridCenter();

        Assert.AreEqual(
            TileState.PermanentBlocked,
            manager.Grid.GetHexTileState(center.x + 7, center.y)
        );
    }

    [Test]
    public void BuildingPlacement_CannotChangeBlockedTiles()
    {
        Assert.Inconclusive(
            "Building placement not implemented yet."
        );
    }

    [Test]
    public void BuildingPlacement_CannotChangePermanentBlockedTiles()
    {
        Assert.Inconclusive(
            "Building placement not implemented yet."
        );
    }
}