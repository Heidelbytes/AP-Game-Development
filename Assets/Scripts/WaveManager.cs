using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;


/// <summary>
/// Controls how enemies are distributed around the spawn center.
/// </summary>
public enum SpawnAngleMode
    {
        FullCircle,   // Spawns 360 degrees around the center
        OneDirection  // Spawns in a focused 90-degree arc 
    }

/// <summary>
/// Stores a display name alongside the prefab used to spawn that enemy.
/// </summary>
[System.Serializable]
public class EnemyPrefab
{   
    public string name;
    public GameObject enemyPrefab;
};

/// <summary>
/// A single enemy type and how many of it should spawn in a wave.
/// </summary>
[System.Serializable]
public class EnemyGroup
{   
    public string name;
    public int amount;
};

/// <summary>
/// One configured wave of enemy spawns.
/// </summary>
[System.Serializable]
public class Wave
{
    public List<EnemyGroup> enemyGroups;
    public SpawnAngleMode angleMode;
    public float targetDirectionAngle;
    public int energyCrystals;
}

/// <summary>
/// Handles wave configuration, spawn direction, and runtime enemy instantiation.
/// </summary>
public class WaveManager : MonoBehaviour
{   
    public static WaveManager Instance;

    [Header("Enemy Waves")]
    [SerializeField] public List<Wave> waves = new List<Wave>();

    [Header("Enemy Prefabs")]
    [SerializeField] public List<EnemyPrefab> Prefabs = new List<EnemyPrefab>();

    private SpawnAngleMode angleMode;

    [Tooltip("The center direction of the spawn arc (0 = Right, 90 = Up/North, 180 = Left, 270 = Down/South).")]
    private float targetDirectionAngle = 90f;

    [Header("Additional Settings")]

    [Header("Distance Settings")]
    [Tooltip("The exact distance from the center (0,0,0) where enemies will spawn.")]
    public float spawnDistance = 25f;
    
    [Header("Environment Alignment")]
    [Tooltip("Drag your ground plane here to ensure enemies spawn flat on its surface.")]
    public MeshRenderer planeRenderer;

    private Transform containerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Starts the selected wave after a short delay to avoid race conditions.
    /// </summary>
    /// <param name="waveNumber">The wave index to spawn.</param>
    public void StartWave(int waveNumber)
    {
        StartCoroutine(WaitForNavMeshBakeSequence(waveNumber));
    }

    /// <summary>
    /// Waits one frame and then spawns the requested wave.
    /// This helps avoid timing issues while terrain generation or NavMesh baking is still running.
    /// </summary>
    /// <param name="waveNumber">The wave index to spawn.</param>
    IEnumerator WaitForNavMeshBakeSequence(int waveNumber)
    {
        yield return null;

        if (this == null)
            yield break;

        Debug.Log("WaveManager: Ground grid processed. Spawning units.");

        SpawnEnemyWave(waveNumber);
    }

    /// <summary>
    /// Returns the parent object used to keep spawned enemies organized in the scene hierarchy.
    /// Creates it if it does not already exist.
    /// </summary>
    private Transform GetSpawnContainer()
    {
        string containerName = "--- Spawned Enemies ---";
        GameObject container = GameObject.Find(containerName);

        if (container == null)
        {
            container = new GameObject(containerName);
            container.transform.SetParent(transform, false);
        }
        else if (container.transform.parent != transform)
        {
            container.transform.SetParent(transform, false);
        }

        return container.transform;
    }

    /// <summary>
    /// Finds the prefab matching the enemy name in the prefab list.
    /// </summary>
    /// <param name="name">Enemy name used in the wave data.</param>
    /// <returns>The matching prefab, or null if none was found.</returns>
    private GameObject GetPrefab(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        foreach (EnemyPrefab currentEnemyPrefab in Prefabs)
        {
            if (currentEnemyPrefab == null)
                continue;

            if (name == currentEnemyPrefab.name)
            {
                return currentEnemyPrefab.enemyPrefab;
            }
        }

        return null;
    }

    /// <summary>
    /// Spawns all enemy groups defined in the selected wave.
    /// </summary>
    /// <param name="waveNumber">The index of the wave to instantiate.</param>
    public void SpawnEnemyWave(int waveNumber)
    {
        if (waves == null || waves.Count == 0 || planeRenderer == null)
        {
            Debug.LogError("Waves or the ground plane renderer are missing!");
            return;
        }

        if (waveNumber < 0 || waveNumber >= waves.Count)
        {
            Debug.LogError("Wave number out of range: " + waveNumber);
            return;
        }

        Wave currentWave = waves[waveNumber];

        if (currentWave == null)
        {
            Debug.LogError("Wave data is missing at index: " + waveNumber);
            return;
        }

        angleMode = currentWave.angleMode;
        targetDirectionAngle = currentWave.targetDirectionAngle;

        containerTransform = GetSpawnContainer();

        float groundSurfaceY = planeRenderer.bounds.max.y;
        Vector3 centerPos = new Vector3(0f, groundSurfaceY, 0f);

        var (minAngleRad, maxAngleRad) = GetSpawnAngleRange();


        if (currentWave.enemyGroups == null)
        {
            Debug.LogWarning("Wave has no enemy groups: " + waveNumber);
            return;
        }

        foreach (EnemyGroup currentEnemyGroup in currentWave.enemyGroups)
        {
            if (currentEnemyGroup == null)
            {
                Debug.LogWarning("Encountered a null enemy group in wave: " + waveNumber);
                continue;
            }

            GameObject currentEnemyPrefab = GetPrefab(currentEnemyGroup.name);

            if (currentEnemyPrefab == null)
            {
                continue;
            }

            if (currentEnemyGroup.amount <= 0)
            {
                continue;
            }

            for (int i = 0; i < currentEnemyGroup.amount; i++)
            {
                float randomAngleRad = Random.Range(minAngleRad, maxAngleRad);
                
                float spawnX = centerPos.x + Mathf.Cos(randomAngleRad) * spawnDistance;
                float spawnZ = centerPos.z + Mathf.Sin(randomAngleRad) * spawnDistance;

                Vector3 spawnPosition = new Vector3(spawnX, groundSurfaceY, spawnZ);

                if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 50f, NavMesh.AllAreas))
                {
                    spawnPosition = hit.position;
                }
                else
                {
                    Vector3 rayStart = spawnPosition + Vector3.up * 5.0f;
                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit physicalHit, 10.0f))
                    {
                        spawnPosition = physicalHit.point;
                    }
                }

                GameObject enemyInstance = Instantiate(currentEnemyPrefab, spawnPosition, Quaternion.identity, containerTransform);

                Vector3 lookTarget = new Vector3(centerPos.x, enemyInstance.transform.position.y, centerPos.z);
                enemyInstance.transform.LookAt(lookTarget);
            }
        }
    }

    /// <summary>
    /// Returns the valid spawn angle range in radians for the current spawn mode.
    /// </summary>
    private (float min, float max) GetSpawnAngleRange()
    {
        if (angleMode == SpawnAngleMode.FullCircle)
        {
            return (0f, Mathf.PI * 2f);
        }

        float normalizedDirection = Mathf.Repeat(targetDirectionAngle, 360f);
        float centerRad = normalizedDirection * Mathf.Deg2Rad;
        float halfSpanRad = 45f * Mathf.Deg2Rad;

        float minAngle = centerRad - halfSpanRad;
        float maxAngle = centerRad + halfSpanRad;

        return (minAngle, maxAngle);
    }

    private void OnDrawGizmosSelected()
    {
        if (planeRenderer == null || spawnDistance <= 0f)
            return;

        DrawSpawnRangeGizmo();
    }

    /// <summary>
    /// Draws the current spawn arc in the editor to visualize enemy entry positions.
    /// </summary>
    private void DrawSpawnRangeGizmo()
    {
        float planeTopY = planeRenderer.bounds.max.y;
        Vector3 centerPos = new Vector3(0f, planeTopY + 0.1f, 0f);

        Gizmos.color = Color.red;
        var (minAngle, maxAngle) = GetSpawnAngleRange();

        int segments = 40;
        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float ratio = (float)i / segments;
            float angle = Mathf.Lerp(minAngle, maxAngle, ratio);

            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 currentPoint = centerPos + new Vector3(cos * spawnDistance, 0, sin * spawnDistance);

            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, currentPoint);
            }

            if (angleMode == SpawnAngleMode.OneDirection && (i == 0 || i == segments))
            {
                Gizmos.DrawLine(centerPos, currentPoint);
            }

            previousPoint = currentPoint;
        }
    }
}