using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour {

    public static TileManager Instance { get; private set; }

    [SerializeField]
    Tilemap worldMap, obstacleMap, previewMap;

    [SerializeField]
    Tile obstacleTile;

    Graph graph;

    ConstructableGraph constructableGraph;
    ConstructableGraph constructablePreviewGraph;

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
        graph = new Graph();
        graph.CreateGraph(obstacleMap);

        constructableGraph = new ConstructableGraph(worldMap, this);

        constructablePreviewGraph = new ConstructableGraph(previewMap, this);
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

    public bool Construct(Vector2Int startPosition, Constructable constructable) {
        int x = startPosition.x;
        int y = startPosition.y;

        // First, check if the desired area is completely clear.
        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                // Found a non-empty tile
                if (worldMap.HasTile(new Vector3Int(x + col, y + row, 0))) return false;
            }
        }

        // The area is clear, so we may continue with construction
        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                SetTile(x + col, y + row, tc.worldTile, tc.obstructive);
                constructableGraph.SetConstructable(new Vector2Int(x + col, y + row), (startPosition, constructable));
            }
        }

        return true;
    }

    public void Destroy(Vector2Int position) {

        // Find the beginning of the constructable which covers the desired position
        (Vector2Int startPos, Constructable constructable) = GetConstructableAt(position);

        if (constructable == null) return;

        int x = startPos.x;
        int y = startPos.y;

        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                SetTile(x + col, y + row, null, false);
                constructableGraph.RemoveConstructable(new Vector2Int(x + col, y + row));
            }
        }
    }

    public void SetPreview(Vector2Int startPosition, Constructable constructable) {
        int x = startPosition.x;
        int y = startPosition.y;

        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                SetPreviewTile(x + col, y + row, tc.previewTile);
                constructablePreviewGraph.SetConstructable(new Vector2Int(x + col, y + row), (startPosition, constructable));
            }
        }
    }

    public void RemovePreview(Vector2Int position) {

        (Vector2Int startPos, Constructable constructable) = GetConstructablePreviewAt(position);

        if (constructable == null) return;

        int x = startPos.x;
        int y = startPos.y;

        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                SetPreviewTile(x + col, y + row, null);
                constructablePreviewGraph.RemoveConstructable(new Vector2Int(x + col, y + row));
            }
        }
    }

    public (Vector2Int, Constructable) GetConstructableAt(Vector2Int position) {
        return constructableGraph.GetConstructable(position);
    }

    public (Vector2Int, Constructable) GetConstructablePreviewAt(Vector2Int position) {
        return constructablePreviewGraph.GetConstructable(position);
    }

    void SetPreviewTile(int x, int y, TileBase t) {
        previewMap.SetTile(new Vector3Int(x, y, 0), t);
    }

    void SetTile(int x, int y, TileBase t, bool obstructive) {
        Vector3Int pos = new Vector3Int(x, y, 0);
        worldMap.SetTile(pos, t);

        if (obstructive) obstacleMap.SetTile(pos, obstacleTile);
        else obstacleMap.SetTile(pos, null);

        graph.SetObstructed(pos.x, pos.y, obstructive);
    }
}
