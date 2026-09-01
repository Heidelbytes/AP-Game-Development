using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


public class HexGridManager : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    [SerializeField] private Transform pfHexagon;
    [SerializeField] private Transform gridParent;
    
    [Header("Grid Settings")]
    [SerializeField] private bool generateGridOnStart = true;
    private bool showGrid = true;

    [Header("Grid Zones")]
    [SerializeField] private int initialBuildRadius = 3;
    [SerializeField] private int maxBuildRadius = 7;
    private int currentBuildRadius;

    [Header("Tile Visual Materials")]
    [SerializeField] private Material freeMaterial;
    [SerializeField] private Material occupiedMaterial;
    [SerializeField] private Material blockedMaterial;
    [SerializeField] private Material permanentBlockedMaterial;

    private GridHexXZ gridHexXZ;


    #region USABLE_FUNCTIONS

    // function to toggle showGrid from the inspector
    [ContextMenu("Toggle Grid Visibility")]
    private void ToggleGridVisibility()
    {
        showGrid = !showGrid;

        if (Application.isPlaying)
        {
            SetGridVisible(showGrid);
        }
    }

    // function to toggle showGrid and update the Grid (True = Grid / False = no Grid)
    public void SetBuildMode(bool active)
    {
        showGrid = active;
        SetGridVisible(active);
    }

    // sets parent object and select prefab to not active
    public void SetGridVisible(bool visible)
    {
        gridParent.gameObject.SetActive(visible);
    }

    // sets TileState of the GridObject and updates the Visual of the HexagonTile
    public void ChangeTileState(int x, int z, TileState newTileState)
    {
        gridHexXZ.SetTileState(x, z, newTileState);
        UpdateTileVisual(x, z);
    }

    // sets new currentBuildRadius and updates the HexTiles and the Visuals
    public void UpgradeBuildRadius(int amountToAdd = 1)
    {
        currentBuildRadius += amountToAdd;
        Vector2Int center = gridHexXZ.GetCenter();

        // get each GridObject and updates state from blocked to free if in new Radius
        for (int x = 0; x < gridHexXZ.GetWidth(); x++)
        {
            for (int z = 0; z < gridHexXZ.GetHeight(); z++)
            {
                GridObject gridObject = gridHexXZ.GetGridObject(x, z);
                if (gridObject == null) 
                    continue;

                int distance = gridHexXZ.GetHexDistance(x, z, center.x, center.y);

                if (gridObject.state == TileState.Blocked && distance <= currentBuildRadius)
                {
                    ChangeTileState(x, z, TileState.Free);
                }
            }
        }
    }


    // Getter
    public GridHexXZ Grid => gridHexXZ;
    public bool HasGrid => gridHexXZ != null;
    public bool ShowGrid => showGrid;


    #endregion




    #region GRID_SETUP
    private void Awake()
    {
        // set BuildRadius
        currentBuildRadius = initialBuildRadius;

        if (generateGridOnStart)
        {
            GenerateGrid();
        }
    }

    private void GenerateGrid()
    {
        if (pfHexagon == null)
        {
            Debug.LogError("pfHexagon is not assigned.");
            return;
        }

        // grid initial values
        int width = 15;
        int height = 17;
        float cellSize = 1f;

        gridHexXZ = new GridHexXZ(width, 
            height, 
            cellSize, 
            new Vector3(-7f, 0.01f, -6f)
        );

        // get the center of the Grid
        Vector2Int center = gridHexXZ.GetCenter();

        // set the hex prefabs
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                Transform visualTransform = Instantiate(
                    pfHexagon, 
                    gridHexXZ.GetWorldPosition(x, z), 
                    Quaternion.Euler(90, 0, 0),
                    gridParent
                    );

                gridHexXZ.GetGridObject(x, z).visualTransform = visualTransform;

                // set initial Tilestates for each HexTile
                TileState initialState = DetermineInitialTileState(x, z, center);
                gridHexXZ.SetTileState(x, z, initialState);
                UpdateTileVisual(x, z); // Set Visual
            }
        }
    }

    // sets the rules for the initial TileState of a given HexTile and the center of the grid
    private TileState DetermineInitialTileState(int x, int z, Vector2Int center)
    {
        int distance = gridHexXZ.GetHexDistance(x, z, center.x, center.y);

        // central HexTiles occupied by the beacon
        if (distance <= 1)
            return TileState.Occupied;

        // outside of buildable map (the forrest)
        if (distance >= maxBuildRadius)
            return TileState.PermanentBlocked;

        // initial buildable HexTiles
        if (distance <= initialBuildRadius)
            return TileState.Free;

        // the rest of the HexTiles is by default blocked until they get unlocked
        return TileState.Blocked;
    }

    #endregion




    #region TILE_STATE/VISUALS
    public void UpdateTileVisual(int x, int z)
    {
        // get GridObject
        GridObject gridObject = gridHexXZ.GetGridObject(x, z);
        if (gridObject.visualTransform == null)
            return;

        // get MeshRenderer
        MeshRenderer renderer = gridObject.visualTransform.GetComponent<MeshRenderer>(); 
        if (renderer == null) 
            return;

        // set the right material for the object state
        switch (gridObject.state)
        {
            case TileState.Free:
                renderer.sharedMaterial = freeMaterial;
                break;
            case TileState.Occupied:
                renderer.sharedMaterial = occupiedMaterial;
                break;
            case TileState.Blocked:
                renderer.sharedMaterial = blockedMaterial;
                break;
            case TileState.PermanentBlocked:
                renderer.sharedMaterial = permanentBlockedMaterial;
                break;
        }

    }


    #endregion

    



    

    // raycasting to get Mouse position on grid
    public static Vector3 GetMouseWorldPosition() {
    return GetMouseWorldPositionOnGround(Mouse.current.position.ReadValue(), Camera.main);
    }

    public static Vector3 GetMouseWorldPositionOnGround(Vector3 screenPosition, Camera worldCamera) {
        Ray ray = worldCamera.ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance)) {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}
