using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Graph : MonoBehaviour {

    Tilemap tilemap;
    
    public int minX, minY, maxX, maxY;

    bool[,] emptyCells;

    public void CreateGraph(Tilemap tilemap) {
        this.tilemap = tilemap;

        //// Get the grid bounds
        Bounds localBounds = tilemap.localBounds;
        // Bottom left corner
        Vector3 p1 = tilemap.transform.TransformPoint(localBounds.min);
        // Top right corner
        Vector3 p2 = tilemap.transform.TransformPoint(localBounds.max);


        // This is super confusing; x- comes before x+, and y- also comes before y+...
        // i.e. y+ does NOT grow down from the top of the screen
        minX = (int) p1.x;
        minY = (int) p1.y;

        maxX = (int) p2.x;
        maxY = (int) p2.y;

        //// Create the array to store the cells
        emptyCells = new bool[maxX - minX, maxY - minY];

        // need to do y, then x (if displaying the debug stuff! I got super confused as to why the image was rotated 90 degrees lol)
        for (int y = maxY ; y > minY ; y -= 1) {
            for (int x = minX ; x < maxX ; x += 1) {

                int xIndex = x - minX;
                int yIndex = maxY - y;

                emptyCells[xIndex, yIndex] = ! tilemap.HasTile(new Vector3Int(x, y, 0));
            }
        }
    }

    public bool IsInBounds(int x, int y) {
        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }

    public bool IsUnobstructed(int x, int y) {
        return emptyCells[x - minX, maxY - y];
    }

    public void SetObstruction(int x, int y, bool value) {
        emptyCells[x, y] = value;
    }
}
