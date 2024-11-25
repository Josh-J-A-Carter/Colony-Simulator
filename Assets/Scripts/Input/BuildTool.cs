using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildTool : Tool {

    [SerializeField]
    Tile previewTile, constructTile;

    TileManager tm => TileManager.Instance;

    Vector2Int previewPoint;
    bool previewActive = false;

    public void Run(HoverData data) {
        Preview(data);

        if (Input.GetKeyDown(KeyCode.Mouse0)) Build(data);
    }

    void Preview(HoverData data) {
        HoverType type = data.GetType();

        if (previewActive && type != HoverType.Tile) {
            tm.SetPreview(previewPoint.x, previewPoint.y, null);
            previewActive = false;
        }

        // We are still hovering over a tile but the preview has moved; so remove the old and update it
        else if (previewActive && type == HoverType.Tile && previewPoint != data.GetTileData()) {
            tm.SetPreview(previewPoint.x, previewPoint.y, null);
            previewPoint = data.GetTileData();
            tm.SetPreview(previewPoint.x, previewPoint.y, previewTile);
        }

        // No active selection, but we need one
        else if (!previewActive && type == HoverType.Tile) {
            previewActive = true;
            previewPoint = data.GetTileData();
            tm.SetPreview(previewPoint.x, previewPoint.y, previewTile);
        }
    }

    void Build(HoverData data) {
        if (data.GetType() != HoverType.Tile) return;

        tm.SetTile(previewPoint.x, previewPoint.y, constructTile, true);
    }

    public void OnDequip() {
        if (previewActive) tm.SetPreview(previewPoint.x, previewPoint.y, null);
    }

}
