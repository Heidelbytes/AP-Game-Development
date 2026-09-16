using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class HexGridManagerPlayModeTests
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
        // Create GameObject with manager
        managerGO = new GameObject("HexGridManager");
        managerGO.SetActive(false);
        manager = managerGO.AddComponent<HexGridManager>();

        // Create dummy materials
        freeMat = new Material(Shader.Find("Standard"));
        occMat = new Material(Shader.Find("Standard"));
        blockMat = new Material(Shader.Find("Standard"));
        permBlockMat = new Material(Shader.Find("Standard"));

        // Create prefab
        GameObject hexPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pfHexagon = hexPrefab.transform;

        // Create parent
        GameObject parentGO = new GameObject("GridParent");
        gridParent = parentGO.transform;

        // Assign inspector fields
        typeof(HexGridManager).GetField("pfHexagon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, pfHexagon);

        typeof(HexGridManager).GetField("gridParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, gridParent);

        typeof(HexGridManager).GetField("freeMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, freeMat);

        typeof(HexGridManager).GetField("occupiedMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, occMat);

        typeof(HexGridManager).GetField("blockedMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, blockMat);

        typeof(HexGridManager).GetField("permanentBlockedMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
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
    public IEnumerator GenerateGrid_CreatesCorrectNumberOfTiles()
    {
        yield return null;

        Assert.IsTrue(manager.HasGrid);
        Assert.AreEqual(15, manager.Grid.GetGridWidth());
        Assert.AreEqual(17, manager.Grid.GetGridHeight());
    }

    [UnityTest]
    public IEnumerator SetBuildMode_TogglesGridVisibility()
    {
        yield return null;

        manager.SetBuildMode(false);
        yield return null;

        Assert.IsFalse(manager.GridParent.gameObject.activeSelf);

        manager.SetBuildMode(true);
        yield return null;

        Assert.IsTrue(manager.GridParent.gameObject.activeSelf);
    }


    [UnityTest]
    public IEnumerator ChangeTileState_UpdatesMaterial()
    {
        yield return null;

        manager.ChangeTileState(5, 5, TileState.Free);
        yield return null;

        var tile = manager.Grid.GetHexTile(5, 5);
        var renderer = tile.visualTransform.GetComponent<MeshRenderer>();

        Assert.AreEqual(freeMat, renderer.sharedMaterial);
    }

    [UnityTest]
    public IEnumerator UpgradeBuildRadius_UnblocksTiles()
    {
        yield return null;

        manager.UpgradeBuildRadius(2);
        yield return null;

        var center = manager.Grid.GetGridCenter();
        int dist = manager.Grid.GetHexTileDistance(center.x + 2, center.y, center.x, center.y);

        var tile = manager.Grid.GetHexTile(center.x + 2, center.y);

        Assert.AreEqual(TileState.Free, tile.state);
    }
}
