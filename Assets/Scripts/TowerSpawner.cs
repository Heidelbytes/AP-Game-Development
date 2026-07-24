using UnityEngine;
using System.Collections.Generic; // Required to use Lists for dynamic data tracking

public class TowerSpawner : MonoBehaviour
{
    // A globally accessible static reference to this specific class instance (Singleton pattern)
    public static TowerSpawner Instance;

    // Defines the selection options for how our towers will layout across the battlefield
    public enum SpawnPattern { PureRandom, RadialRing }

    [Header("Spawning Mode")]
    // The currently active placement behavior editable inside the Unity Inspector
    public SpawnPattern activePattern = SpawnPattern.PureRandom;

    [Header("Central Target")]
    // The core objective prefab that sits at the center of the defensive grid
    public GameObject centralTargetPrefab;

    [Header("Defending Towers")]
    // Array holding the various different types of defense tower prefabs we can choose from
    public GameObject[] defensiveTowerPrefabs;
    
    // Total number of defensive structures we want to attempt to generate in the world
    public int numberOfTowers = 10;
    
    // The clearance radius required around any given tower to prevent overlapping clusters
    public float minDistanceBetweenTowers = 4f;

    [Header("Radial Pattern Settings")]
    // The closest distance a defense tower can spawn relative to the central base
    public float innerRadius = 5f;
    
    // The maximum distance outward a defense tower can spawn in radial mode
    public float outerRadius = 20f;

    [Header("Environment")]
    // Reference to the floor object's mesh to read real-world bounds and height metrics
    public MeshRenderer planeRenderer;

    // Internal list keeping track of every active game object we have placed in the scene
    private List<GameObject> spawnedTowers = new List<GameObject>();
    
    // Stores a direct reference to the instantiated central core game object
    private GameObject centralTargetInstance;

    // Called when the script instance is being loaded before Start() runs
    private void Awake()
    {
        // If our static instance variable isn't assigned yet, assign it to this script
        if (Instance == null) { Instance = this; }
        // If an instance already exists elsewhere, destroy this copy to prevent duplicates
        else { Destroy(gameObject); }
    }

    // Executed on the frame when the script is enabled just before update loops trigger
    void Start()
    {
        // 1. Core Logic: Run the calculations and spawn the central target and protective walls
        SpawnDefenseLayout();

        // 2. Locate the navigation baking component currently active in your hierarchy
        RuntimeNavMesh runtimeNav = FindObjectOfType<RuntimeNavMesh>();
        
        // If the runtime navigation script is found safely in the open scene
        if (runtimeNav != null)
        {
            // Command the navigation system to bake the walkable paths around the new obstacles
            runtimeNav.BakeMapAtRuntime();
        }
        else // If the script cannot be found anywhere in the current scene hierarchy
        {
            // Output a warning to the console so you know the map geometry changed but paths didn't update
            Debug.LogWarning("TowerSpawner: Could not find RuntimeNavMesh in the scene to bake the paths!");
        }
    }

    // Handles the setup loop, error verification, and physical generation of your defenses
    public void SpawnDefenseLayout()
    {
        // Clean out any existing towers from previous matches to start with a fresh slate
        ClearActiveTowers();

        // Safety check: verify that a floor surface was passed into the slot in the inspector
        if (planeRenderer == null)
        {
            // Terminate placement execution early and notify the developer via an error log
            Debug.LogError("PlaneRenderer is not assigned!");
            return;
        }

        // Fetch the absolute highest Y coordinate value along the top face of the floor surface
        float planeTopY = planeRenderer.bounds.max.y;
        
        // Establish our world spatial origin point sitting flush against the floor center
        Vector3 centerPosition = new Vector3(0f, planeTopY, 0f);

        // Spawn Central Target if a matching prefab configuration has been provided
        if (centralTargetPrefab != null)
        {
            // Materialize the central objective object into the scene directly at our origin point
            centralTargetInstance = Instantiate(centralTargetPrefab, centerPosition, Quaternion.identity);
            
            // Assign the identity tag needed by enemy AI scripts to locate the main target
            centralTargetInstance.tag = "CoreBeacon";
            
            // Offset the object upward so its physical base rests perfectly flush on top of the floor
            AdjustObjectHeightToSurface(centralTargetInstance, planeTopY);
            
            // Register this centerpiece object inside our master collection to avoid tower overlaps
            spawnedTowers.Add(centralTargetInstance);
        }

        // Safety Check: check if the defensive tower library is empty or unassigned entirely
        if (defensiveTowerPrefabs == null || defensiveTowerPrefabs.Length == 0)
        {
            // Halt program execution immediately to prevent crash loops when selecting indices
            Debug.LogError("No defensive tower prefabs assigned!");
            return;
        }

        // Variable tracking the number of structures successfully placed in valid coordinates
        int successfullySpawned = 0;
        
        // Safety threshold limiting generation iterations to prevent infinite loops if space runs out
        int maxAttempts = numberOfTowers * 150; 
        
        // Accumulator counting total calculation loops processed during layout assembly
        int attempts = 0;

        // Keep trying to locate valid positions until target count is reached or safety threshold triggers
        while (successfullySpawned < numberOfTowers && attempts < maxAttempts)
        {
            // Track this evaluation pass toward our maximum ceiling limit
            attempts++;
            
            // Generate a coordinate candidate based on the current active spatial pattern selection
            Vector3 potentialPosition = GetPotentialSpawnPosition(centerPosition, planeTopY);

            // Run collision check loops to see if this point satisfies our clear spacing rule
            if (IsValidPosition(potentialPosition))
            {
                // Select a randomized index across our list of allowed defensive building types
                GameObject selectedPrefab = defensiveTowerPrefabs[Random.Range(0, defensiveTowerPrefabs.Length)];
                
                // Construct the selected building instance directly at the cleared coordinates
                GameObject newTower = Instantiate(selectedPrefab, potentialPosition, Quaternion.identity);
                
                // Tag the new building so that enemy navigation treats it as a targetable obstruction
                newTower.tag = "Building"; 
                
                // Offset the asset vertically so it doesn't clip below or hover above the plane
                AdjustObjectHeightToSurface(newTower, planeTopY);
                
                // Append the brand new asset reference into the active obstruction collection
                spawnedTowers.Add(newTower);
                
                // Increment our successful placement tracker by one unit
                successfullySpawned++;
            }
        }

        // If the generation loops timed out before hitting your requested structural target count
        if (successfullySpawned < numberOfTowers)
        {
            // Log a warning specifying how many items were dropped due to spacing restrictions
            Debug.LogWarning($"Could only fit {successfullySpawned}/{numberOfTowers} towers using {activePattern} mode.");
        }
    }

    // Ensures that objects rest cleanly on top of surfaces based on their bottom boundaries
    private void AdjustObjectHeightToSurface(GameObject obj, float planeTopY)
    {
        // Tracks how far down the object's lowest boundary sits relative to its pivot transform point
        float bottomOffset = 0f;

        // Strategy A: If the object has a physical collision shape attached
        if (obj.TryGetComponent<Collider>(out Collider col))
        {
            // Calculate distance between the central transform position and the lowest boundary point
            bottomOffset = obj.transform.position.y - col.bounds.min.y;
        }
        // Strategy B: Fallback check if a collider is missing but rendering bounds exist
        else if (obj.TryGetComponent<MeshRenderer>(out MeshRenderer ren))
        {
            // Extract spatial offsets from the lowest edge of the visual model instead
            bottomOffset = obj.transform.position.y - ren.bounds.min.y;
        }

        // Grab current transform positions to safely keep the X and Z placements unmodified
        Vector3 currentPos = obj.transform.position;
        
        // Relocate the object using the floor height combined with our precise pivot spacing offset
        obj.transform.position = new Vector3(currentPos.x, planeTopY + bottomOffset, currentPos.z);
    }

    // Returns a raw coordinate candidate depending on the active procedural configuration selected
    private Vector3 GetPotentialSpawnPosition(Vector3 centerPos, float spawnY)
    {
        switch (activePattern)
        {
            // Dynamic Strategy A: Sifting coordinates inside a donut-shaped layout boundary
            case SpawnPattern.RadialRing:
                // Pick a completely random direction along a 360-degree radial plane using Radians
                float angle = Random.Range(0f, Mathf.PI * 2f);
                
                // Use a square root mapping to guarantee an even area distribution of points within the ring
                float radius = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));
                
                // Convert polar variables (Angle + Radius) back into standard Cartesian coordinates (X, Y, Z)
                return new Vector3(centerPos.x + Mathf.Cos(angle) * radius, spawnY, centerPos.z + Mathf.Sin(angle) * radius);

            // Dynamic Strategy B: Standard rectangular coordinates covering the full surface area
            case SpawnPattern.PureRandom:
            default: // Default case handles execution if any edge case states occur
                // Extract bounding container box dimensions from our linked environment mesh
                Bounds planeBounds = planeRenderer.bounds;
                
                // Select a randomized horizontal floating point coordinate spanning the full width
                float randomX = Random.Range(planeBounds.min.x, planeBounds.max.x);
                
                // Select a randomized vertical depth coordinate spanning the full length
                float randomZ = Random.Range(planeBounds.min.z, planeBounds.max.z);
                
                // Build a finalized positioning vector safely locking our height axis value
                return new Vector3(randomX, spawnY, randomZ);
        }
    }

    // Scans through our history index to ensure incoming objects don't intersect existing elements
    private bool IsValidPosition(Vector3 position)
    {
        // Loop over every entry inside our tracked array structure sequentially
        foreach (GameObject tower in spawnedTowers)
        {
            // Confirm the object entry is active and has not been cleared or unassigned
            if (tower != null)
            {
                // Measure the absolute length separation using purely 2D horizontal planes (ignoring elevation spikes)
                float distance = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(tower.transform.position.x, tower.transform.position.z));
                
                // If the computed space separation drops below our specified clearance range boundary
                if (distance < minDistanceBetweenTowers)
                {
                    // Return failure status immediately to reject the candidate position coordinate
                    return false;
                }
            }
        }
        // If the coordinate candidate passes all spacing checks safely
        return true;
    }

    // Disposes of all active elements in the scene hierarchy to allow clean system rewrites
    public void ClearActiveTowers()
    {
        // Loop through the historical sequence array collection step by step
        foreach (GameObject tower in spawnedTowers)
        {
            // Wipe the specified active GameObject completely from system memory assets
            if (tower != null) Destroy(tower);
        }
        // Purge all references stored in our tracking list container to drop length back to zero
        spawnedTowers.Clear();
        
        // Wipe the core base game object cleanly from the active scene structure
        if (centralTargetInstance != null) Destroy(centralTargetInstance);
    }

    // Automatically runs inside the Unity Editor screen environment whenever this component is active and highlighted
    private void OnDrawGizmosSelected()
    {
        // Stop execution cleanly if there is no floor geometry reference available to measure boundaries from
        if (planeRenderer == null) return;

        // Identify the exact top elevation level of our floor surface asset layout
        float planeTopY = planeRenderer.bounds.max.y;
        
        // Push the wireframe elevation up slightly (+0.1) to avoid flickering issues with matching ground planes
        float renderHeight = planeTopY + 0.1f; 
        
        // Establish our coordinate tracking origin reference point inside the scene grid layout
        Vector3 centerPos = new Vector3(0f, renderHeight, 0f);

        // Assign green coloring to our oncoming debug helper lines
        Gizmos.color = Color.green;

        // If the configuration behavior is set to fill the entire rectangular boundary
        if (activePattern == SpawnPattern.PureRandom)
        {
            // Extract the spatial bounding container values directly from our environment floor mesh renderer
            Bounds bounds = planeRenderer.bounds;
            
            // Build out a thin horizontal flat matching sizing vector representing the perimeter dimensions
            Vector3 size = new Vector3(bounds.size.x, 0.1f, bounds.size.z);
            
            // Define our alignment tracking anchor point directly matching the physical floor mesh setup
            Vector3 center = new Vector3(bounds.center.x, renderHeight, bounds.center.z);
            
            // Render a flat wireframe box outline inside the editor viewport representing the valid spawn zone
            Gizmos.DrawWireCube(center, size);
        }
        // If the configuration behavior is locked onto a circular layout restriction shape instead
        else if (activePattern == SpawnPattern.RadialRing)
        {
            // Number of individual connecting vector segments used to cleanly shape our drawn loops
            int segments = 64; 
            
            // Draw the inner boundary perimeter limit line loop using our helper function
            DrawGizmoCircle(centerPos, innerRadius, segments);
            
            // Draw the outer boundary perimeter limit line loop using our helper function
            DrawGizmoCircle(centerPos, outerRadius, segments);
        }
    }

    // Internal calculation math tool to cleanly step through coordinates forming circular rings
    private void DrawGizmoCircle(Vector3 center, float radius, int segments)
    {
        // Calculate the initial starting vector coordinate located on the outer right rim edge of the ring
        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

        // Process line calculations connecting piece by piece around the circumference edge perimeter loop
        for (int i = 1; i <= segments; i++)
        {
            // Determine our progress percentage across the full loop traversal process
            float r = (float)i / segments;
            
            // Convert that linear percentage progress directly into matching circular radian units (0 to 2*PI)
            float angle = r * Mathf.PI * 2f;

            // Compute the next coordinate step point position along our circle line using trigonometry
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            
            // Draw a straight color line link connecting our previous tracked step position to our new target position
            Gizmos.DrawLine(previousPoint, nextPoint);
            
            // Save our new coordinate tracking values to act as the beginning point for the oncoming layout step
            previousPoint = nextPoint;
        }
    }
}