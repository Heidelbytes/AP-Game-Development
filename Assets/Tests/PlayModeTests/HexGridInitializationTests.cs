using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/*
 * Requirement:
 * REQ-000 Hex-Grid Initialization 
 
*/




public class HexGridInitializationTests
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

    [UnityTest]
    public IEnumerator Grid_IsGenerated_OnStartup()
    {
        yield return null;

        Assert.IsTrue(manager.HasGrid);
        Assert.IsNotNull(manager.Grid);
    }

    [UnityTest]
    public IEnumerator Grid_HasExpectedDimensions()
    {
        yield return null;

        Assert.AreEqual(15, manager.Grid.GetGridWidth());
        Assert.AreEqual(17, manager.Grid.GetGridHeight());
    }

    [UnityTest]
    public IEnumerator EveryTile_HasValidCoordinates()
    {
        yield return null;

        for (int x = 0; x < manager.Grid.GetGridWidth(); x++)
        {
            for (int z = 0; z < manager.Grid.GetGridHeight(); z++)
            {
                GridObject tile = manager.Grid.GetHexTile(x, z);

                Assert.IsNotNull(tile);
            }
        }
    }

    [UnityTest]
    public IEnumerator EveryTile_HasVisualReference()
    {
        yield return null;

        for (int x = 0; x < manager.Grid.GetGridWidth(); x++)
        {
            for (int z = 0; z < manager.Grid.GetGridHeight(); z++)
            {
                GridObject tile = manager.Grid.GetHexTile(x, z);

                Assert.IsNotNull(tile.visualTransform);
            }
        }
    }

    [UnityTest]
    public IEnumerator EveryTile_HasInitialTileState()
    {
        yield return null;

        for (int x = 0; x < manager.Grid.GetGridWidth(); x++)
        {
            for (int z = 0; z < manager.Grid.GetGridHeight(); z++)
            {
                GridObject tile = manager.Grid.GetHexTile(x, z);

                Assert.IsTrue(
                    tile.state == TileState.Free ||
                    tile.state == TileState.Occupied ||
                    tile.state == TileState.Blocked ||
                    tile.state == TileState.PermanentBlocked
                );
            }
        }
    }

    [UnityTest]
    public IEnumerator AllTiles_AccessibleThroughGridManager()
    {
        yield return null;

        for (int x = 0; x < manager.Grid.GetGridWidth(); x++)
        {
            for (int z = 0; z < manager.Grid.GetGridHeight(); z++)
            {
                Assert.DoesNotThrow(() =>
                {
                    manager.Grid.GetHexTile(x, z);
                });
            }
        }
    }
}
