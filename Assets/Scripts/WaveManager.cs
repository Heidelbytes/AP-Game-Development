using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;


public enum SpawnAngleMode
    {
        FullCircle,   // Spawns 360 degrees around the center
        OneDirection  // Spawns in a focused 90-degree arc 
    }

[System.Serializable]
public class EnemyPrefab
{   
    public string name;
    public GameObject enemyPrefab;
};

[System.Serializable]
public class EnemyGroup
{   
    public string name;
    public int amount;
};

[System.Serializable]
public class wave
{
    public List<EnemyGroup> enemyGroups;
    public SpawnAngleMode angleMode;
    public float targetDirectionAngle;
    public int energyCrystals;

};

public class WaveManager : MonoBehaviour
{   
    public static WaveManager Instance;

    [Header("Enemy Waves")]
    [SerializeField] public List<wave> waves = new List<wave>();

    [Header("Enemy Prefabs")]
    [SerializeField] public List<EnemyPrefab> Prefabs = new List<EnemyPrefab>();

    private SpawnAngleMode angleMode;

    [Tooltip("The center direction of the spawn arc (0 = Right, 90 = Up/North, 180 = Left, 270 = Down/South).")]
    private float targetDirectionAngle = 90f;

    [Header("                   ")]
    [Header("-------------------")]
    [Header("Additional Settings")]
    [Header("-------------------")]
    
    [Header("Distance Settings")]
    [Tooltip("The exact distance from the center (0,0,0) where enemies will spawn.")]
    public float spawnDistance = 25f;
    
    [Header("Environment Alignment")]
    [Tooltip("Drag your ground plane here to ensure enemies spawn flat on its surface.")]
    public MeshRenderer planeRenderer;

    // Direct reference to the parent container transform in the Hierarchy
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

    public void startWave(int waveNumber) 
    {
        StartCoroutine(WaitForNavMeshBakeSequence(waveNumber));
    }

    // Prepares and retrieves the parent folder object in the Hierarchy
    private Transform GetOrCreateContainer()
    {
        string containerName = "--- Spawned Enemies ---";
        GameObject container = GameObject.Find(containerName);

        if (container == null)
        {
            container = new GameObject(containerName);
        }

        return container.transform;
    }

    // Coroutine that delays execution to avoid race conditions with map/tower generation
    IEnumerator WaitForNavMeshBakeSequence(int waveNumber)
    {
        yield return null;

        Debug.Log("WaveManager: Ground grid processed. Spawning units.");

        SpawnEnemyWave(waveNumber);
    }

    private GameObject getEnemyPrefabUsingName(string name) 
    {
        foreach(EnemyPrefab currentEnemyPrefab in Prefabs) 
        {
            if(name == currentEnemyPrefab.name) {
                return currentEnemyPrefab.enemyPrefab;
            }
        }

        return null;
    }

    // Handles picking positions, correcting height, instantiating, and rotating the enemies
    public void SpawnEnemyWave(int waveNumber)
    {
        if (waves == null || waves.Count == 0 || planeRenderer == null)
        {
            Debug.LogError("Waves or the ground plane renderer are mssing!");
            return;
        }

        // Initiate current wave
        wave currentWave = waves[waveNumber]; 
        angleMode = currentWave.angleMode;
        targetDirectionAngle = currentWave.targetDirectionAngle;

        // Fetch or create the container folder in the hierarchy
        containerTransform = GetOrCreateContainer();

        float groundSurfaceY = planeRenderer.bounds.max.y;
        Vector3 centerPos = new Vector3(0f, groundSurfaceY, 0f);

        var (minAngleRad, maxAngleRad) = GetAngleBoundariesInRadians();


        // 3. Loop through each group of enemies
        foreach (EnemyGroup currentEnemyGroup in currentWave.enemyGroups)
        {   
            GameObject currentEnemyPrefab = getEnemyPrefabUsingName(currentEnemyGroup.name);

            // Skip if no prefab is assigned to avoid errors
            if (currentEnemyPrefab == null) continue;

            // 4. Spawn the specific amount of enemies requested for this group
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

                // Instantiate the specific prefab for this group directly as a child of containerTransform
                GameObject enemyInstance = Instantiate(currentEnemyPrefab, spawnPosition, Quaternion.identity, containerTransform);

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