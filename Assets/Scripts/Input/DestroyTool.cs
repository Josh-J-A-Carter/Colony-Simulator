using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestroyTool : Tool {

    [SerializeField]
    TileBase previewTile;

    TileManager tm => TileManager.Instance;

    Vector2Int previewPoint;
    bool previewActive = false;

    public override void Run(HoverData data) {
        Preview(data);

        if (Input.GetKeyDown(KeyCode.Mouse0)) Destroy(data);
    }

    void Preview(HoverData data) {
        HoverType type = data.GetType();

        if (previewActive && type != HoverType.Tile) {
            tm.RemovePreview(previewPoint);
            previewActive = false;
        }

        // We are still hovering over a tile but the preview has moved; so remove the old and update it
        else if (previewActive && type == HoverType.Tile && previewPoint != data.GetTileData()) {
            tm.RemovePreview(previewPoint);

            previewPoint = data.GetTileData();
            // tm.SetPreview(previewPoint, preview);
        }

        // No active selection, but we need one
        else if (!previewActive && type == HoverType.Tile) {
            previewActive = true;
            previewPoint = data.GetTileData();
            // tm.SetPreview(previewPoint, preview);
        }
    }

    void Destroy(HoverData data) {
        if (data.GetType() != HoverType.Tile) return;

        tm.Destroy(previewPoint);
    }

    public override void OnDequip() {
        if (previewActive) tm.RemovePreview(previewPoint);
    }

}
