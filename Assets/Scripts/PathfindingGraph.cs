using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingGraph : MonoBehaviour {

    [SerializeField]
    Tilemap gameWorld, obstacles;

    [SerializeField]
    int minX, minY, maxX, maxY;

    bool[,] walkableCells;


    // Start is called before the first frame update
    void Start() {

        //// Get the grid bounds
        Bounds localBounds = gameWorld.localBounds;
        // Bottom left corner
        Vector3 p1 = gameWorld.transform.TransformPoint(localBounds.min);
        // Top right corner
        Vector3 p2 = gameWorld.transform.TransformPoint(localBounds.max);


        // This is super confusing; x- comes before x+, and y- also comes before y+...
        // i.e. y+ does NOT grow down from the top of the screen
        minX = (int) p1.x;
        minY = (int) p1.y;

        maxX = (int) p2.x;
        maxY = (int) p2.y;

        //// Create the array to store the cells
        walkableCells = new bool[maxX - minX, maxY - minY];

        // need to do y, then x (if displaying the debug stuff! I got super confused as to why the image was rotated 90 degrees lol)
        for (int y = maxY ; y > minY ; y -= 1) {

            // String currLog = "";
            for (int x = minX ; x < maxX ; x += 1) {

                int xIndex = x - minX;
                int yIndex = maxY - y;

                walkableCells[xIndex, yIndex] = ! obstacles.HasTile(new Vector3Int(x, y, 0));
                // currLog = currLog + " " + (walkableCells[xIndex, yIndex] ? "X" : ". ");
            }
            // Debug.Log(currLog);
        }
    }
}
