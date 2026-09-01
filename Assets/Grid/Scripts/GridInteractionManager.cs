using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInteractionManager : MonoBehaviour
{

    [SerializeField] private HexGridManager gridManager;
    [SerializeField] private GridPlacementManager gridPlacementManager;

    [SerializeField] private Transform pfselectedHex;

    private Transform selectedTransform;
    private int lastX = -999;
    private int lastZ = -999;



    void Awake()
    {
        // instantiate selection hex
        selectedTransform = Instantiate(pfselectedHex);
        selectedTransform.gameObject.SetActive(false);

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
        else
        {
            selectedTransform.gameObject.SetActive(true);
        }

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


        // Input click managment
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            gridPlacementManager.TryPlaceTower(x, z);
        }
        

        // return if still on the same hex
        if (x == lastX && z == lastZ)
            return;

        ///////  Debug TestCode   ///////
        Debug.Log("Hex changed:");
        Debug.Log("x = " + x);
        Debug.Log("z = " + z);
        Debug.Log(gridManager.Grid.GetTileState(x, z));


        // Neues Hex holen
        GridObject current = gridManager.Grid.GetGridObject(x, z);

        // move selection hex
        if (current != null) 
        {
        selectedTransform.gameObject.SetActive(true);
        selectedTransform.position = current.visualTransform.position;
        }
        
        // save
        lastX = x;
        lastZ = z;

    }


    private void OnGUI()
    {
        // Button oben rechts
        if (GUI.Button(new Rect(Screen.width - 180, 20, 160, 40), "Radius +1"))
        {
            gridManager.UpgradeBuildRadius();
        }
    }


}