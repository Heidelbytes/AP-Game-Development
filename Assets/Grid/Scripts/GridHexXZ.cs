using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridHexXZ {
    
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private GridObject[,] gridArray;
    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;


    public GridHexXZ(int width, int height, float cellSize, Vector3 originPosition) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new GridObject[width, height];

        for (int x=0; x<gridArray.GetLength(0); x++) {      
            for (int z=0; z<gridArray.GetLength(1); z++) {

                // create new GridObject with the positions and set it in the array
                GridObject gridObject = new GridObject(x, z);
                gridArray[x, z] = gridObject;       
            }
        }
    }

    public int GetWidth(){return this.width;}

    public int GetHeight(){return this.height;}

    public Vector3 GetWorldPosition(int x, int z)
    {
    float xOffset = (z % 2 == 1) ? cellSize * 0.5f : 0f;

    return new Vector3(
        x * cellSize + xOffset,
        0f,
        z * (cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER)
        ) + originPosition;
    }

    // Approximates the hex coordinate by rounding, then checks all neighbors to find the closest actual hexagon tile
    public void GetXZ(Vector3 worldPosition, out int x, out int z) {
        // get rough hex position
        int roughX = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize);
        int roughZ = Mathf.RoundToInt((worldPosition - originPosition).z / cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER);

        Vector3Int roughXZ = new Vector3Int(roughX, 0, roughZ);

        bool oddRow = roughZ % 2 == 1;

        // get the six neighbours
        List<Vector3Int> neighbourXZList = new List<Vector3Int> {
            roughXZ + new Vector3Int(-1, 0, 0),                     
            roughXZ + new Vector3Int(+1, 0, 0),                     

            roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, +1),      
            roughXZ + new Vector3Int(+0, 0, +1),    

            roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, -1),
            roughXZ + new Vector3Int(+0, 0, -1),
        };

        Vector3Int closestXZ = roughXZ;

        //check which is the nearest neighbor and choose it as the hexagon
        foreach (Vector3Int neighbourXZ in neighbourXZList) {
            if (Vector3.Distance(worldPosition, GetWorldPosition(neighbourXZ.x, neighbourXZ.z)) <
                Vector3.Distance(worldPosition, GetWorldPosition(closestXZ.x, closestXZ.z))) {
                    //Closer than closest
                    closestXZ = neighbourXZ;
                }
        }

        x = closestXZ.x;
        z = closestXZ.z;
    }

    public bool IsInsideGrid(int x, int z){
        return x >= 0 && z >= 0 && x < width && z < height;
    }

    public GridObject GetGridObject(int x, int z) {
        if (IsInsideGrid(x, z)) {
            return gridArray[x, z];
        } else {
            throw new ArgumentOutOfRangeException();
        }
    }

    public GridObject GetGridObject(Vector3 worldPosition) {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        return GetGridObject(x, z);
    }

    public Vector2Int GetCenter() {
        return new Vector2Int(width/2, height/2);
    }

    public int GetHexDistance(int x1, int z1, int x2, int z2)
    {
        // first Hexagon coordinates
        // remove the vertical offset so that mathematical the Hexagons are in a perfect line 
        int rowOffset1 = z1 / 2;
        int adjustedX1 = x1 - rowOffset1;
        int adjustedZ1 = z1;
        // additional axis to account for the extra edges of a Hexagon
        int derivedY1 = -adjustedX1 - adjustedZ1;

        // second Hexagon coordinates
        int rowOffset2 = z2 / 2;
        int adjustedX2 = x2 - rowOffset2;
        int adjustedZ2 = z2;
        int derivedY2 = -adjustedX2 - adjustedZ2;

        //Max differenz of the X, Z, and Y axis as Step count between Hexagon Tiles
        return Mathf.Max(
            Mathf.Abs(adjustedX1 - adjustedX2),
            Mathf.Abs(derivedY1 - derivedY2),
            Mathf.Abs(adjustedZ1 - adjustedZ2)
        );
    }

    public void SetGridObject(int x, int z, GridObject value) {
        if (IsInsideGrid(x, z)){
            gridArray[x, z] = value;
            TriggerGridObjectChanged(x, z);
        } else {
            throw new ArgumentOutOfRangeException();
        }
    }

    public void SetGridObject(Vector3 worldPosition, GridObject value) {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        SetGridObject(x, z, value);
    }

    public void SetTileState(int x, int z, TileState state){
        if (!IsInsideGrid(x, z)) 
            throw new ArgumentOutOfRangeException();
        
        gridArray[x, z].state = state;
        TriggerGridObjectChanged(x, z);
    }

    public TileState GetTileState(int x, int z){
        if (!IsInsideGrid(x, z))
            throw new ArgumentOutOfRangeException();

        return gridArray[x, z].state;
    }



    public void TriggerGridObjectChanged(int x, int z) {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
    }

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int z;
    }

}