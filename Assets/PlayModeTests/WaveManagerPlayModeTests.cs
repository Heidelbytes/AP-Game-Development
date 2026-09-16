using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaveManagerPlayModeTests
{
    private GameObject managerObject;
    private WaveManager manager;
    private GameObject planeObject;
    private GameObject prefabObject;

    [SetUp]
    public void SetUp()
    {
        managerObject = new GameObject("WaveManager Test");
        manager = managerObject.AddComponent<WaveManager>();

        planeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        planeObject.name = "Test Ground";
        planeObject.transform.position = new Vector3(0f, 0f, 0f);
        planeObject.transform.localScale = new Vector3(100f, 2f, 100f);
        manager.planeRenderer = planeObject.GetComponent<MeshRenderer>();

        prefabObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        prefabObject.name = "Test Enemy Prefab";
        prefabObject.SetActive(false);
    }

    [TearDown]
    public void TearDown()
    {
        if (WaveManager.Instance == manager)
        {
            WaveManager.Instance = null;
        }

        Object.Destroy(managerObject);
        Object.Destroy(planeObject);
        Object.Destroy(prefabObject);

        GameObject container = GameObject.Find("--- Spawned Enemies ---");
        if (container != null)
        {
            Object.Destroy(container);
        }
    }

    [UnityTest]
    public IEnumerator StartWave_SpawnsAfterOneFrame()
    {
        manager.waves = new List<Wave> { CreateWave("Test", 1) };
        manager.Prefabs = new List<EnemyPrefab> { CreatePrefab("Test", prefabObject) };

        manager.StartWave(0);
        Assert.IsNull(GameObject.Find("--- Spawned Enemies ---"));

        yield return null;

        GameObject container = GameObject.Find("--- Spawned Enemies ---");
        Assert.IsNotNull(container);
        Assert.AreEqual(1, container.transform.childCount);
    }

    private Wave CreateWave(string enemyName, int amount)
    {
        return new Wave
        {
            angleMode = SpawnAngleMode.FullCircle,
            enemyGroups = new List<EnemyGroup>
            {
                new EnemyGroup { name = enemyName, amount = amount }
            }
        };
    }

    private EnemyPrefab CreatePrefab(string enemyName, GameObject prefab)
    {
        return new EnemyPrefab { name = enemyName, enemyPrefab = prefab };
    }
}
