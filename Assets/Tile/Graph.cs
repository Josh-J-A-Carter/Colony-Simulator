using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Graph {
    
    int minX, minY, maxX, maxY;

    HashSet<Vector2Int> obstacles;

    public void CreateGraph(int minX, int minY, int maxX, int maxY) {
        this.minX = minX;
        this.minY = minY;

        this.maxX = maxX;
        this.maxY = maxY;

        obstacles = new();
    }
    

    public bool IsObstructed(Vector2Int position) {
        return obstacles.Contains(position);
    }

    public bool IsInBounds(int x, int y) {
        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }

    public void SetObstructed(Vector2Int position, bool value) {
        if (value == false) {
            obstacles.Remove(position);
            return;
        }

        obstacles.Add(position);
    }
}
