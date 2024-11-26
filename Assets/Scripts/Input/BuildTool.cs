using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildTool : Tool {

    // [SerializeField]
    Constructable constructable => parent.currentConstructable;

    TileManager tm => TileManager.Instance;

    Vector2Int previewPoint;
    bool previewActive = false;

    public override void Run(HoverData data) {
        Preview(data);

        if (Input.GetKeyDown(KeyCode.Mouse0)) Build(data);
    }

    void Preview(HoverData data) {
        HoverType type = data.GetType();

        if (previewActive && type != HoverType.Tile) {
            DestroyPreview();
        }

        // We are still hovering over a tile but the preview has moved; so remove the old and update it
        else if (previewActive && type == HoverType.Tile && previewPoint != data.GetTileData()) {
            DestroyPreview();

            previewPoint = data.GetTileData();
            ConstructPreview();
        }

        // No active selection, but we need one
        else if (!previewActive && type == HoverType.Tile) {
            previewPoint = data.GetTileData();
            
            ConstructPreview();
        }
    }

    void ConstructPreview() {
        previewActive = true;

        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                tm.SetPreview(previewPoint.x + col, previewPoint.y + row, tc.previewTile);
            }
        }
    }

    void DestroyPreview() {
        previewActive = false;

        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                tm.SetPreview(previewPoint.x + col, previewPoint.y + row, null);
            }
        }
    }

    void Build(HoverData data) {
        if (data.GetType() != HoverType.Tile) return;

        Construct();
    }

    void Construct() {
        for (int row = 0; row < constructable.RowCount() ; row += 1) {
            Row rowData = constructable.GetRow(row);

            for (int col = 0; col < rowData.tileConstructs.Length; col += 1) {
                TileConstruct tc = rowData.tileConstructs[col];
                // Ignore empty constructs
                if (tc.worldTile == null) continue;

                tm.SetTile(previewPoint.x + col, previewPoint.y + row, tc.worldTile, tc.obstructive);
            }
        }
    }

    public void OnDequip() {
        if (previewActive) tm.SetPreview(previewPoint.x, previewPoint.y, null);
    }

}
