using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour {

    public static TileManager Instance { get; private set; }

    [SerializeField]
    Tilemap worldMap, obstaclesMap, previewMap;
    Graph graph;

    void Awake() {
        // Instantiate singleton
        if (Instance == null) Instance = (TileManager) this;
        else if (Instance != this) {
            Destroy(this);
            return;
        }

        Instantiate();
    }

    void Instantiate() {
        graph = GetComponent<Graph>();
        graph.CreateGraph(obstaclesMap);
    }

    public bool IsInBounds(int x, int y) {
        return graph.IsInBounds(x, y);
    }

    public bool IsInBounds(Vector2Int p) {
        return graph.IsInBounds(p.x, p.y);
    }

    public bool IsUnobstructed(int x, int y) {
        return graph.IsUnobstructed(x, y);
    }

    public bool IsUnobstructed(Vector2Int p) {
        return graph.IsUnobstructed(p.x, p.y);
    }

    // public void SetObstruction(int x, int y, bool value) {
    //     graph.SetObstruction(x, y, value);
    // }

    // public void SetObstruction(Vector2Int p, bool value) {
    //     graph.SetObstruction(p.x, p.y, value);
    // }
}
