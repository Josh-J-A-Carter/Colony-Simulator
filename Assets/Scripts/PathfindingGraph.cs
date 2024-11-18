using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingGraph : MonoBehaviour {


    static readonly int CARDINAL_DIR_COST = 10;
    static readonly int DIAGONAL_DIR_COST = 14;

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



        Debug.Log("Find path between (0, 0) and (3, 3): ");

        List<Vector2Int> path = FindPath(new Vector2(0, 0), new Vector2(3, 3));

        String str = "";
        foreach (Vector2Int v in path) str = str + "    (" + v.x + ", " + v.y + ")";
        Debug.Log(str);
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


        // ...


        return neighbours;
    }

    bool IsInBounds(int x, int y) {
        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }

    bool IsUnobstructed(int x, int y) {
        return walkableCells[x - minX, maxY - y];
    }

    class Node {
        public Vector2Int point { get; }
        public Node parent { get; }
        public int cost { get; }

        public Node(Vector2Int point, Node parent, int cost) {
            this.point = point;
            this.parent = parent;
            this.cost = cost;
        }
    }

    public List<Vector2Int> FindPath(Vector2 startPoint, Vector2 endPoint) {
        Vector2Int root = new Vector2Int((int) startPoint.x, (int) startPoint.y);
        Vector2Int goal = new Vector2Int((int) endPoint.x, (int) endPoint.y);

        if (!IsInBounds(goal.x, goal.y) || !IsUnobstructed(goal.x, goal.y)) return null;

        List<Node> queue = new List<Node>();
        List<Node> found = new List<Node>();

        queue.Add(new Node(root, null, 0));

        Node destination = null;
        while (queue.Count > 0) {
            // Get the next node to visit
            Node next = Dequeue(queue);

            // Are we done?
            if (next.point == goal) {
                destination = next;
                break;
            }

            // Have we already seen this point?
            bool seen = false;
            foreach (Node node in found) {
                if (node.point == next.point) {
                    seen = true;
                    break;
                }
            }

            if (seen) continue;

            // This must be a genuinely-new point to visit, so add its neighbours
            foreach ((Vector2Int neighbour, int cost) in GetNeighbours(next.point)) {
                queue.Add(new Node(neighbour, next, next.cost + cost));
            }
        }

        // No path was found :(
        if (destination == null) return null;

        // A path was found, so convert the linked list of Node instances into List<Vector2Int>
        List<Vector2Int> path = new List<Vector2Int>();

        Node current = destination;
        while (current.point != root) {
            path.Add(current.point);
            current = current.parent;
        }

        path.Reverse();

        return path;
    }

    Node Dequeue(List<Node> queue) {
        int minIndex = 0;
        int minCost = queue[minIndex].cost;

        int index = 0;
        while (index < queue.Count - 1) {
            index += 1;
            if (queue[index].cost < minCost) minIndex = index;
        }

        Node next = queue[minIndex];
        queue.RemoveAt(minIndex);

        return next;
    }
}
