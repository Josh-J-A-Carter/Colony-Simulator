using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfind {

    static TileManager tm = TileManager.Instance;

    static readonly int CARDINAL_DIR_COST = 10;
    static readonly int DIAGONAL_DIR_COST = 14;

    /// <summary>
    /// Find all the neighbouring tiles and their cost to travel to each of them.
    /// </summary>
    /// <returns>List of tuples, each containing a neighbour and its associated travel cost / weight</returns>
    static List<(Vector2Int, int)> GetNeighbours(Vector2Int point) {
        List<(Vector2Int, int)> neighbours = new List<(Vector2Int, int)>();

        int x = point.x;
        int y = point.y;

        // Neighbouring cells in cardinal directions
        if (tm.IsInBounds(x - 1, y) && tm.IsUnobstructed(x - 1, y)) neighbours.Add((new Vector2Int(x - 1, y), CARDINAL_DIR_COST));
        if (tm.IsInBounds(x + 1, y) && tm.IsUnobstructed(x + 1, y)) neighbours.Add((new Vector2Int(x + 1, y), CARDINAL_DIR_COST));
        if (tm.IsInBounds(x, y - 1) && tm.IsUnobstructed(x, y - 1)) neighbours.Add((new Vector2Int(x, y - 1), CARDINAL_DIR_COST));
        if (tm.IsInBounds(x, y + 1) && tm.IsUnobstructed(x, y + 1)) neighbours.Add((new Vector2Int(x, y + 1), CARDINAL_DIR_COST));

        // // Diagonals
        // // Can only go to diagonal tile (x*, y*) if:
        // // - (x*, y*) is in the grid bounds
        // // - (x*, y*) is not obstructed
        // // - there is an unobstructed cardinal cell next to it (otherwise, we could slip through corners in walls)
        if (tm.IsInBounds(x - 1, y - 1) && tm.IsUnobstructed(x - 1, y - 1)
            && (tm.IsUnobstructed(x - 1, y) || tm.IsUnobstructed(x, y - 1))) neighbours.Add((new Vector2Int(x - 1, y - 1), DIAGONAL_DIR_COST));
        if (tm.IsInBounds(x - 1, y + 1) && tm.IsUnobstructed(x - 1, y + 1)
            && (tm.IsUnobstructed(x - 1, y) || tm.IsUnobstructed(x, y + 1))) neighbours.Add((new Vector2Int(x - 1, y + 1), DIAGONAL_DIR_COST));
        if (tm.IsInBounds(x + 1, y + 1) && tm.IsUnobstructed(x + 1, y + 1)
            && (tm.IsUnobstructed(x + 1, y) || tm.IsUnobstructed(x, y + 1))) neighbours.Add((new Vector2Int(x + 1, y + 1), DIAGONAL_DIR_COST));
        if (tm.IsInBounds(x + 1, y - 1) && tm.IsUnobstructed(x + 1, y - 1)
            && (tm.IsUnobstructed(x + 1, y) || tm.IsUnobstructed(x, y - 1))) neighbours.Add((new Vector2Int(x + 1, y - 1), DIAGONAL_DIR_COST));

        return neighbours;
    }

    /// <summary>
    /// Heuristic function in A* algorithm (Straight line distance)
    /// </summary>
    /// <returns>Distance, multiplied by 10 and truncated</returns>
    static int CalculateHeuristic(Vector2Int p1, Vector2Int p2) {
        return (int) (10 * Vector2Int.Distance(p1, p2));
    }

    /// <summary>
    /// Find a path, if it exists, between a start point and an end point, in the tile grid.
    /// </summary>
    /// <returns>A valid path between <c>startPoint</c> and <c>endPoint</c> if one exists; 
    /// or returns <c>null</c> if no such path exists.</returns>
    public static Path FindPath(Vector2 startPoint, Vector2 endPoint) {
        Vector2Int root = new Vector2Int((int) Math.Floor(startPoint.x), (int) Math.Floor(startPoint.y));
        Vector2Int goal = new Vector2Int((int) Math.Floor(endPoint.x), (int) Math.Floor(endPoint.y));

        HashSet<Vector2Int> openSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScores = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> fScores = new Dictionary<Vector2Int, int>();

        if (!tm.IsInBounds(goal.x, goal.y) || !tm.IsUnobstructed(goal.x, goal.y)) return null;

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
        List<Vector2Int> path = new List<Vector2Int> { goal };

        Vector2Int current = goal;
        Vector2Int parent;

        while (parents.TryGetValue(current, out parent)) {
            path.Add(parent);
            current = parent;
        }

        path.Reverse();

        return new Path(path);
    }


    /// <summary>
    /// Get the next point from the open set, with the lowest fScore.
    /// </summary>
    static Vector2Int GetNext(HashSet<Vector2Int> openSet, Dictionary<Vector2Int, int> fScores) {
        
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
