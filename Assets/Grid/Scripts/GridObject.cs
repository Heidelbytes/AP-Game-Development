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
    public Transform visualTransform;
    public TileState state = TileState.Free;

    //TODO
    //public Tower placedTower;
}