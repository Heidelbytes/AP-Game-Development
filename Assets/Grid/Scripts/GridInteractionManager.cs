using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInteractionManager : MonoBehaviour
{
    [SerializeField] private HexGridManager gridManager;
    [SerializeField] private GridPlacementManager gridPlacementManager;
    [SerializeField] private Transform pfselectedHex;
    [SerializeField] private Material buildableSelected;
    [SerializeField] private Material notBuildableSelected;


    private Transform selectedTransform;
    private MeshRenderer selectedRenderer;
    private int lastX = -999;
    private int lastZ = -999;



    void Awake()
    {
        // instantiate selection hex
        selectedTransform = Instantiate(pfselectedHex);
        selectedTransform.gameObject.SetActive(false);

        // set selected mesh renderer at the start to change color later
        selectedRenderer = selectedTransform.GetComponent<MeshRenderer>();

    }


    // Update is called once per frame
    void Update()
    {
        // when no Grid exists return
        if (!gridManager.HasGrid)
            return;
        
        // when Grid is invisible set selectedHex invisible
        if (!gridManager.ShowGrid)
        {
            selectedTransform.gameObject.SetActive(false);
            return;
        } 
        // else set it as active
        selectedTransform.gameObject.SetActive(true);
        


        // get the mous Postion on the Grid
        Vector3 mouseWorldPosition = HexGridManager.GetMouseWorldPosition();
        // Get the Grid Koordinates
        int x, z;
        gridManager.Grid.GetXZ(mouseWorldPosition, out x, out z);


        // out of bounce
        if (!gridManager.Grid.IsInsideGrid(x, z))
        {
            selectedTransform.gameObject.SetActive(false);
            return;
        }

        // Input click
        HandleClick(x, z);


        ///////  Debug TestCode   ///////
        Debug.Log("Hex changed:");
        Debug.Log("x = " + x);
        Debug.Log("z = " + z);
        Debug.Log(gridManager.Grid.GetTileState(x, z));

        // move SelectedHex
        HandleHover(x, z);

        

    }





    // Input click managment
    private void HandleClick(int x, int z)
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            gridPlacementManager.TryPlaceTower(x, z);
        }
    }

    private void HandleHover(int x, int z)
    {
        // Neues Hex holen
        GridObject current = gridManager.Grid.GetGridObject(x, z);
        if (current == null) 
            return;
        
        
        // check Mesh Renderer of selection Tile and set Material for buildable or notBuildable
        if (selectedRenderer == null) 
            return;

        bool buildable = current.state == TileState.Free;
        if (buildable)
        {
            selectedRenderer.sharedMaterial = buildableSelected;
        }
        else
        {
            selectedRenderer.sharedMaterial = notBuildableSelected;
        }

        // return if still on the same hex
        if (x == lastX && z == lastZ)
            return;

        // move selection HexTile
        selectedTransform.position = current.visualTransform.position;

        // save
        lastX = x;
        lastZ = z;
    }




    ////// TestButton for the BuildRadius //////
    private void OnGUI()
    {
        // Button oben rechts
        if (GUI.Button(new Rect(Screen.width - 180, 20, 160, 40), "Radius +1"))
        {
            gridManager.UpgradeBuildRadius();
        }
    }


}