using UnityEngine;

public class GridPlacementManager : MonoBehaviour
{
    [SerializeField] private HexGridManager gridManager;
    [SerializeField] private Transform towerPF;
   





    public bool IsBuildable(int x, int z)
    {
        TileState state = gridManager.Grid.GetTileState(x, z);
        return state == TileState.Free;
    }



    public void TryPlaceTower(int x, int z) 
    {
        if (!IsBuildable(x, z))
        {
            Debug.Log("cant be placed");
            return;
        }

        GridObject current = gridManager.Grid.GetGridObject(x, z);



        Instantiate(towerPF, current.visualTransform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);

        gridManager.ChangeTileState(x, z, TileState.Occupied);

        Debug.Log("Cylinder placed at " + x + ", " + z);
    }


}
