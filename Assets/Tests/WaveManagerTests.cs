using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaveManagerTests
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

        Object.DestroyImmediate(managerObject);
        Object.DestroyImmediate(planeObject);
        Object.DestroyImmediate(prefabObject);

        GameObject container = GameObject.Find("--- Spawned Enemies ---");
        if (container != null)
        {
            Object.DestroyImmediate(container);
        }
    }

    [Test]
    public void SpawnAngleRange_FullCircle_CoversFullCircle()
    {
        SetPrivateField("angleMode", SpawnAngleMode.FullCircle);

        (float min, float max) range = GetSpawnAngleRange();

        Assert.AreEqual(0f, range.min);
        Assert.AreEqual(Mathf.PI * 2f, range.max, 0.0001f);
    }

    [Test]
    public void SpawnAngleRange_OneDirection_NormalizesAngles()
    {
        SetPrivateField("angleMode", SpawnAngleMode.OneDirection);
        SetPrivateField("targetDirectionAngle", 450f);

        (float min, float max) range = GetSpawnAngleRange();

        Assert.AreEqual(45f * Mathf.Deg2Rad, range.min, 0.0001f);
        Assert.AreEqual(135f * Mathf.Deg2Rad, range.max, 0.0001f);
    }

    [Test]
    public void SpawnEnemyWave_InvalidWaveNumber_DoesNotSpawnEnemies()
    {
        manager.waves = new List<Wave> { CreateWave("Test", 2) };
        manager.Prefabs = new List<EnemyPrefab> { CreatePrefab("Test", prefabObject) };

        LogAssert.Expect(LogType.Error, "Wave number out of range: 4");
        manager.SpawnEnemyWave(4);

        Assert.IsNull(GameObject.Find("--- Spawned Enemies ---"));
    }

    [Test]
    public void SpawnEnemyWave_MissingGround_DoesNotSpawnEnemies()
    {
        manager.planeRenderer = null;
        manager.waves = new List<Wave> { CreateWave("Test", 2) };
        manager.Prefabs = new List<EnemyPrefab> { CreatePrefab("Test", prefabObject) };

        LogAssert.Expect(LogType.Error, "Waves or the ground plane renderer are missing!");
        manager.SpawnEnemyWave(0);

        Assert.IsNull(GameObject.Find("--- Spawned Enemies ---"));
    }

    [Test]
    public void SpawnEnemyWave_SpawnsConfiguredAmountUnderContainer()
    {
        manager.spawnDistance = 10f;
        manager.waves = new List<Wave> { CreateWave("Test", 3) };
        manager.Prefabs = new List<EnemyPrefab> { CreatePrefab("Test", prefabObject) };

        manager.SpawnEnemyWave(0);

        GameObject container = GameObject.Find("--- Spawned Enemies ---");
        Assert.IsNotNull(container);
        Assert.AreEqual(managerObject.transform, container.transform.parent);
        Assert.AreEqual(3, container.transform.childCount);

        for (int i = 0; i < container.transform.childCount; i++)
        {
            Transform enemy = container.transform.GetChild(i);
            Assert.AreEqual(manager.spawnDistance, new Vector2(enemy.position.x, enemy.position.z).magnitude, 0.1f);
            Assert.AreEqual(manager.planeRenderer.bounds.max.y, enemy.position.y, 0.1f);
        }
    }

    [Test]
    public void SpawnEnemyWave_OneDirection_StaysWithinConfiguredArc()
    {
        manager.spawnDistance = 10f;
        manager.waves = new List<Wave>
        {
            new Wave
            {
                angleMode = SpawnAngleMode.OneDirection,
                targetDirectionAngle = 90f,
                enemyGroups = new List<EnemyGroup>
                {
                    new EnemyGroup { name = "Test", amount = 20 }
                }
            }
        };
        manager.Prefabs = new List<EnemyPrefab> { CreatePrefab("Test", prefabObject) };

        manager.SpawnEnemyWave(0);

        GameObject container = GameObject.Find("--- Spawned Enemies ---");
        Assert.AreEqual(20, container.transform.childCount);

        for (int i = 0; i < container.transform.childCount; i++)
        {
            Vector3 offset = container.transform.GetChild(i).position;
            Assert.GreaterOrEqual(offset.z, 0f);
            Assert.LessOrEqual(Mathf.Abs(offset.x), offset.z + 0.01f);
        }
    }

    [Test]
    public void SpawnEnemyWave_SkipsUnknownAndNonPositiveGroups()
    {
        manager.waves = new List<Wave>
        {
            new Wave
            {
                angleMode = SpawnAngleMode.FullCircle,
                enemyGroups = new List<EnemyGroup>
                {
                    new EnemyGroup { name = "Unknown", amount = 5 },
                    new EnemyGroup { name = "Test", amount = 0 },
                    new EnemyGroup { name = "Test", amount = -1 }
                }
            }
        };
        manager.Prefabs = new List<EnemyPrefab> { CreatePrefab("Test", prefabObject) };

        manager.SpawnEnemyWave(0);

        GameObject container = GameObject.Find("--- Spawned Enemies ---");
        Assert.IsNotNull(container);
        Assert.AreEqual(0, container.transform.childCount);
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

    private (float min, float max) GetSpawnAngleRange()
    {
        MethodInfo method = typeof(WaveManager).GetMethod(
            "GetSpawnAngleRange",
            BindingFlags.Instance | BindingFlags.NonPublic);

        return ((float min, float max))method.Invoke(manager, null);
    }

    private void SetPrivateField(string fieldName, object value)
    {
        FieldInfo field = typeof(WaveManager).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        field.SetValue(manager, value);
    }
}