using UnityEngine;
using System.Collections;
using UnityEngine.AI;

// 1. Create a serializable class to hold the prefab and its specific spawn count
[System.Serializable]
public class EnemySpawnGroup
{
    public GameObject enemyPrefab;
    public int spawnCount;
}

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnAngleMode
    {
        FullCircle,   // Spawns 360 degrees around the center
        OneDirection  // Spawns in a focused 90-degree arc 
    }

    [Header("Enemy Waves & Counts")]
    [Tooltip("Define which enemies to spawn and how many of each.")]
    // 2. Replace the old array and spawnCount with an array of our new class
    public EnemySpawnGroup[] enemySpawnGroups;
    
    [Header("Angle Settings")]
    public SpawnAngleMode angleMode = SpawnAngleMode.FullCircle;

    [Range(0f, 360f)]
    [Tooltip("The center direction of the spawn arc (0 = Right, 90 = Up/North, 180 = Left, 270 = Down/South).")]
    public float targetDirectionAngle = 90f;

    [Header("Distance Settings")]
    [Tooltip("The exact distance from the center (0,0,0) where enemies will spawn.")]
    public float spawnDistance = 25f;
    
    [Header("Environment Alignment")]
    [Tooltip("Drag your ground plane here to ensure enemies spawn flat on its surface.")]
    public MeshRenderer planeRenderer;

    void Start()
    {
        StartCoroutine(WaitForNavMeshBakeSequence());
    }

    // Coroutine that delays execution to avoid race conditions with map/tower generation
    IEnumerator WaitForNavMeshBakeSequence()
    {
        yield return null;

        Debug.Log("EnemySpawner: Ground grid processed. Spawning units.");
        SpawnEnemyWave();
    }

    // Handles picking positions, correcting height, instantiating, and rotating the enemies
    public void SpawnEnemyWave()
    {
        if (enemySpawnGroups == null || enemySpawnGroups.Length == 0 || planeRenderer == null)
        {
            Debug.LogError("EnemySpawner is missing spawn groups or the ground plane renderer!");
            return;
        }

        float groundSurfaceY = planeRenderer.bounds.max.y;
        Vector3 centerPos = new Vector3(0f, groundSurfaceY, 0f);

        var (minAngleRad, maxAngleRad) = GetAngleBoundariesInRadians();

        // 3. Loop through each group of enemies
        foreach (EnemySpawnGroup group in enemySpawnGroups)
        {
            // Skip if no prefab is assigned to avoid errors
            if (group.enemyPrefab == null) continue;

            // 4. Spawn the specific amount of enemies requested for this group
            for (int i = 0; i < group.spawnCount; i++)
            {
                float randomAngleRad = Random.Range(minAngleRad, maxAngleRad);
                
                float spawnX = centerPos.x + Mathf.Cos(randomAngleRad) * spawnDistance;
                float spawnZ = centerPos.z + Mathf.Sin(randomAngleRad) * spawnDistance;

                Vector3 spawnPosition = new Vector3(spawnX, groundSurfaceY, spawnZ);

                if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
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

                // Instantiate the specific prefab for this group
                GameObject enemyInstance = Instantiate(group.enemyPrefab, spawnPosition, Quaternion.identity);

                Vector3 lookTarget = new Vector3(centerPos.x, enemyInstance.transform.position.y, centerPos.z);
                enemyInstance.transform.LookAt(lookTarget);
            }
        }
    }

    private (float min, float max) GetAngleBoundariesInRadians()
    {
        if (angleMode == SpawnAngleMode.FullCircle)
        {
            return (0f, Mathf.PI * 2f);
        }

        float centerRad = targetDirectionAngle * Mathf.Deg2Rad;
        float halfSpanRad = 45f * Mathf.Deg2Rad;

        return (centerRad - halfSpanRad, centerRad + halfSpanRad);
    }

    private void OnDrawGizmosSelected()
    {
        if (planeRenderer == null) return;

        float planeTopY = planeRenderer.bounds.max.y;
        Vector3 centerPos = new Vector3(0f, planeTopY + 0.1f, 0f);

        Gizmos.color = Color.red; 
        var (minAngle, maxAngle) = GetAngleBoundariesInRadians();

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