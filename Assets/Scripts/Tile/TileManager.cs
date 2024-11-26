using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour {

    public static TileManager Instance { get; private set; }

    [SerializeField]
    Tilemap worldMap, obstacleMap, previewMap;

    [SerializeField]
    Tile obstacleTile;

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
        graph.CreateGraph(obstacleMap);
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

    public void SetPreview(int x, int y, TileBase t) {
        previewMap.SetTile(new Vector3Int(x, y, 0), t);
    }

    public void SetTile(int x, int y, TileBase t, bool obstructive) {
        Vector3Int pos = new Vector3Int(x, y, 0);
        worldMap.SetTile(pos, t);

        if (obstructive) obstacleMap.SetTile(pos, obstacleTile);
        else obstacleMap.SetTile(pos, null);

        graph.SetObstructed(pos.x, pos.y, obstructive);
    }
}
