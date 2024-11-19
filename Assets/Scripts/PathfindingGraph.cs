using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingGraph : MonoBehaviour {


    static readonly int CARDINAL_DIR_COST = 10;
    static readonly int DIAGONAL_DIR_COST = 14;

    [SerializeField]
    Tilemap gameWorld, obstacles, pathVisualiser;

    [SerializeField]
    Tile wood;

    [SerializeField]
    public int minX, minY, maxX, maxY;

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
            for (int x = minX ; x < maxX ; x += 1) {

                int xIndex = x - minX;
                int yIndex = maxY - y;

                walkableCells[xIndex, yIndex] = ! obstacles.HasTile(new Vector3Int(x, y, 0));
            }
        }
    }

    public void VisualisePath(List<Vector2Int> path) {
        foreach (Vector2Int v in path) pathVisualiser.SetTile(new Vector3Int(v.x, v.y, 0), wood);
    }

    public void DevisualisePath(List<Vector2Int> path) {
        foreach (Vector2Int v in path) pathVisualiser.SetTile(new Vector3Int(v.x, v.y, 0), null);
    }
    List<(Vector2Int, int)> GetNeighbours(Vector2Int point) {
        List<(Vector2Int, int)> neighbours = new List<(Vector2Int, int)>();

        int x = point.x;
        int y = point.y;

        // Neighbouring cells in cardinal directions
        if (IsInBounds(x - 1, y) && IsUnobstructed(x - 1, y)) neighbours.Add((new Vector2Int(x - 1, y), CARDINAL_DIR_COST));
        if (IsInBounds(x + 1, y) && IsUnobstructed(x + 1, y)) neighbours.Add((new Vector2Int(x + 1, y), CARDINAL_DIR_COST));
        if (IsInBounds(x, y - 1) && IsUnobstructed(x, y - 1)) neighbours.Add((new Vector2Int(x, y - 1), CARDINAL_DIR_COST));
        if (IsInBounds(x, y + 1) && IsUnobstructed(x, y + 1)) neighbours.Add((new Vector2Int(x, y + 1), CARDINAL_DIR_COST));

        // // Diagonals
        // // Can only go to diagonal tile (x*, y*) if:
        // // - (x*, y*) is in the grid bounds
        // // - (x*, y*) is not obstructed
        // // - there is an unobstructed cardinal cell next to it (otherwise, we could slip through corners in walls)
        if (IsInBounds(x - 1, y - 1) && IsUnobstructed(x - 1, y - 1)
            && (IsUnobstructed(x - 1, y) || IsUnobstructed(x, y - 1))) neighbours.Add((new Vector2Int(x - 1, y - 1), DIAGONAL_DIR_COST));
        if (IsInBounds(x - 1, y + 1) && IsUnobstructed(x - 1, y + 1)
            && (IsUnobstructed(x - 1, y) || IsUnobstructed(x, y + 1))) neighbours.Add((new Vector2Int(x - 1, y + 1), DIAGONAL_DIR_COST));
        if (IsInBounds(x + 1, y + 1) && IsUnobstructed(x + 1, y + 1)
            && (IsUnobstructed(x + 1, y) || IsUnobstructed(x, y + 1))) neighbours.Add((new Vector2Int(x + 1, y + 1), DIAGONAL_DIR_COST));
        if (IsInBounds(x + 1, y - 1) && IsUnobstructed(x + 1, y - 1)
            && (IsUnobstructed(x + 1, y) || IsUnobstructed(x, y - 1))) neighbours.Add((new Vector2Int(x + 1, y - 1), DIAGONAL_DIR_COST));

        return neighbours;
    }

    public bool IsInBounds(int x, int y) {
        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }

    public bool IsUnobstructed(int x, int y) {
        return walkableCells[x - minX, maxY - y];
    }

    int CalculateHeuristic(Vector2Int p1, Vector2Int p2) {
        return (int) (10 * Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2)));
    }

    public List<Vector2Int> FindPath(Vector2 startPoint, Vector2 endPoint) {
        Vector2Int root = new Vector2Int((int) startPoint.x, (int) startPoint.y);
        Vector2Int goal = new Vector2Int((int) endPoint.x, (int) endPoint.y);

        HashSet<Vector2Int> openSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScores = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> fScores = new Dictionary<Vector2Int, int>();

        if (!IsInBounds(goal.x, goal.y) || !IsUnobstructed(goal.x, goal.y)) return null;

        openSet.Add(root);
        gScores.Add(root, 0);
        fScores.Add(root, 0);

        bool found = false;
        while (openSet.Count > 0) {
            
            // Get the next node to visit
            Vector2Int next = GetNext(openSet, fScores);
            openSet.Remove(next);
            closedSet.Add(next);

            // Are we done?
            if (next == goal) {
                found = true;
                break;
            }

            // This must be a genuinely-new point to visit, so add its neighbours
            int gScore;
            gScores.TryGetValue(next, out gScore);

            foreach ((Vector2Int neighbour, int edgeWeight) in GetNeighbours(next)) {

                // Have we already found the best path to this point?
                if (closedSet.Contains(neighbour)) continue;

                // Have we got a better tentative answer in the open set?
                int neighbourGScore = gScore + edgeWeight;
                int existingGScore;
                if (fScores.TryGetValue(neighbour, out existingGScore) && existingGScore < neighbourGScore) continue;

                // No (better) tentative answer thus far, so add the node
                openSet.Add(neighbour);
                parents[neighbour] = next;
                gScores[neighbour] = neighbourGScore;
                int neighbourHScore = CalculateHeuristic(neighbour, goal);
                fScores[neighbour] = neighbourGScore + neighbourHScore;
            }
        }

        // No path was found :(
        if (!found) return null;

        // A path was found, so convert the linked list of Node instances into List<Vector2Int>
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(goal);

        Vector2Int current = goal;
        Vector2Int parent;

        while (parents.TryGetValue(current, out parent)) {
            path.Add(parent);
            current = parent;
        }

        path.Reverse();

        return path;
    }


    Vector2Int GetNext(HashSet<Vector2Int> openSet, Dictionary<Vector2Int, int> fScores) {
        
        Vector2Int optimum = openSet.ElementAt(0);
        int optimalCost;
        fScores.TryGetValue(optimum, out optimalCost);

        foreach (Vector2Int current in openSet) {
            int currentCost;
            if (fScores.TryGetValue(current, out currentCost) && currentCost < optimalCost) {
                optimum = current;
                optimalCost = currentCost;
            }
        }

        return optimum;
    }
}
