using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectTool : Tool {

    [SerializeField]
    Constructable preview;

    TileManager tm => TileManager.Instance;

    Vector2Int previewPoint;
    bool previewActive = false;

    public override void Run(HoverData data) {
        HoverType type = data.GetType();

        Vector2Int newPreviewPoint = data.GetTileData();

        if (previewActive && type != HoverType.Tile) {
            tm.RemovePreview(previewPoint);
            previewActive = false;
        }

        // We are still hovering over a tile but the preview has moved; so remove the old and update it
        else if (previewActive && type == HoverType.Tile && previewPoint != newPreviewPoint) {
            tm.RemovePreview(previewPoint);
            previewPoint = newPreviewPoint;
            tm.SetPreview(previewPoint, preview);
        }

        // No active selection, but we need one
        else if (!previewActive && type == HoverType.Tile) {
            previewActive = true;
            previewPoint = newPreviewPoint;
            tm.SetPreview(previewPoint, preview);
        }
    }

    public override void OnDequip() {
        if (previewActive) tm.RemovePreview(previewPoint);
    }

}
