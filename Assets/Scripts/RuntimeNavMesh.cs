using UnityEngine;
using Unity.AI.Navigation; // Required for NavMeshSurface

[RequireComponent(typeof(NavMeshSurface))]
public class RuntimeNavMesh : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    // Called automatically by the TowerSpawner once the environment layout settles
    public void BakeMapAtRuntime()
    {
        if (navMeshSurface != null)
        {
            Debug.Log("NavMesh: Baking paths around runtime towers...");
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("RuntimeNavMesh: Missing NavMeshSurface component on this GameObject!");
        }
    }
}