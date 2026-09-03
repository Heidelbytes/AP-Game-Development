using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Represents a 2D hexagonal grid using offset coordinates
public class HexGrid {
    
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private GridObject[,] hexTileArray;
    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;


    public HexGrid(int width, int height, float cellSize, Vector3 originPosition) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        hexTileArray = new GridObject[width, height];

        for (int x=0; x<hexTileArray.GetLength(0); x++) {      
            for (int z=0; z<hexTileArray.GetLength(1); z++) {

                // create new GridObject with the positions and set it in the array
                GridObject gridObject = new GridObject(x, z);
                hexTileArray[x, z] = gridObject;       
            }
        }
    }


    #region GRID_INFO / GETTERS
    public int GetGridWidth(){return this.width;}

    public int GetGridHeight(){return this.height;}

    public bool IsInsideGrid(int x, int z){
        return x >= 0 && z >= 0 && x < width && z < height;
    }

    public Vector2Int GetGridCenter() {
        return new Vector2Int(width/2, height/2);
    }

    #endregion



    
    #region COORDINATES / POSITION

    // Converts hex tile coordinates (x, z) into Unity world space.
    // odd rows are horizontally offset by half a cell.
    public Vector3 GetHexTileWorldPosition(int x, int z)
    {
    float xOffset = (z % 2 == 1) ? cellSize * 0.5f : 0f;

    return new Vector3(
        x * cellSize + xOffset,
        0f,
        z * (cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER)
        ) + originPosition;
    }

    // Converts a world position into the closest hex tile
    // 1: Estimate tile by rounding
    // 2: Check all 6 neighbors to find the closest actual tile
    public void WorldToHexTile(Vector3 worldPosition, out int x, out int z) {

        // get rough hex position in grid for estimat tile
        int roughX = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize);
        int roughZ = Mathf.RoundToInt((worldPosition - originPosition).z / cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER);

        Vector3Int roughXZ = new Vector3Int(roughX, 0, roughZ);

        bool oddRow = roughZ % 2 == 1;

        // get the six neighbours
        List<Vector3Int> hexNeighborCoordinates = new List<Vector3Int> {
            // horizontal neighbors 
            roughXZ + new Vector3Int(-1, 0, 0), // left            
            roughXZ + new Vector3Int(+1, 0, 0), // right                  

            // Upper row neighbors (odd/even row offset)
            roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, +1),  // upper diagonal
            roughXZ + new Vector3Int(+0, 0, +1),    // upper    

            roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, -1),  // lower diagonal
            roughXZ + new Vector3Int(+0, 0, -1),    // lower
        };

        Vector3Int closestHexTileCoordinates = roughXZ;

        //check which is the nearest neighbor and choose it as the hexagon
        foreach (Vector3Int hexNeighborPositions in hexNeighborCoordinates) {
            if (Vector3.Distance(worldPosition, GetHexTileWorldPosition(hexNeighborPositions.x, hexNeighborPositions.z)) <
                Vector3.Distance(worldPosition, GetHexTileWorldPosition(closestHexTileCoordinates.x, closestHexTileCoordinates.z))) {
                    //Closer than closest
                    closestHexTileCoordinates = hexNeighborPositions;
                }
        }

        x = closestHexTileCoordinates.x;
        z = closestHexTileCoordinates.z;
    }

    // calculate distance between two hex tiles using cube coordinates
    // offset coordinates are converted into cube (x,y,z) before computing distance
    public int GetHexTileDistance(int x1, int z1, int x2, int z2)
    {
        // first Hexagon coordinates
        // remove the vertical offset so that the tiles are in one line (convert to cube coordinates)
        int rowOffset1 = z1 / 2;
        int adjustedX1 = x1 - rowOffset1;
        int adjustedZ1 = z1;
        // additional axis to account for the extra edges of a Hexagon
        int derivedY1 = -adjustedX1 - adjustedZ1;

        // second Hexagon coordinates
        int rowOffset2 = z2 / 2;
        int adjustedX2 = x2 - rowOffset2;
        int adjustedZ2 = z2;
        // additional axis to account for the extra edges of a Hexagon
        int derivedY2 = -adjustedX2 - adjustedZ2;

        //Max differenz of the X, Z, and Y axis as Step count between Hexagon Tiles
        return Mathf.Max(
            Mathf.Abs(adjustedX1 - adjustedX2),
            Mathf.Abs(derivedY1 - derivedY2),
            Mathf.Abs(adjustedZ1 - adjustedZ2)
        );
    }

    #endregion




    #region GRID_OBJECT / TILE_STATE
    public GridObject GetHexTile(int x, int z) {
        if (IsInsideGrid(x, z)) {
            return hexTileArray[x, z];
        } else {
            throw new ArgumentOutOfRangeException();
        }
    }

    public GridObject GetHexTile(Vector3 worldPosition) {
        int x, z;
        WorldToHexTile(worldPosition, out x, out z);
        return GetHexTile(x, z);
    }

    public void SetHexTile(int x, int z, GridObject value) {
        if (IsInsideGrid(x, z)){
            hexTileArray[x, z] = value;
            NotifyHexTileUpdated(x, z);
        } else {
            throw new ArgumentOutOfRangeException();
        }
    }

    public void SetHexTile(Vector3 worldPosition, GridObject value) {
        int x, z;
        WorldToHexTile(worldPosition, out x, out z);
        SetHexTile(x, z, value);
    }

    public void SetHexTileState(int x, int z, TileState state){
        if (!IsInsideGrid(x, z)) 
            throw new ArgumentOutOfRangeException();
        
        hexTileArray[x, z].state = state;
        NotifyHexTileUpdated(x, z);
    }

    public TileState GetHexTileState(int x, int z){
        if (!IsInsideGrid(x, z))
            throw new ArgumentOutOfRangeException();

        return hexTileArray[x, z].state;
    }

    #endregion




    // Fires when a tile changes (state or object replaced)
    public void NotifyHexTileUpdated(int x, int z) {
        OnGridObjectChanged?.Invoke(this, new HexTileUpdatedEventArgs { x = x, z = z });
    }

    public event EventHandler<HexTileUpdatedEventArgs> OnGridObjectChanged;
    public class HexTileUpdatedEventArgs : EventArgs {
        public int x;
        public int z;
    }

}