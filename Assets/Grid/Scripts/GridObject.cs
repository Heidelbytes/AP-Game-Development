using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileState
{
    Free,
    Occupied,
    Blocked,
    PermanentBlocked
}

public class GridObject
{
    public int x;
    public int z;
    public Transform visualTransform;
    public TileState state = TileState.Free;

    //TODO
    //public Tower placedTower;


    public GridObject(int x, int z) {
        this.x = x;
        this.z = z;
    }
}