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
        HoverType type = data.GetHoverType();

        Vector2Int newPreviewPoint = data.GetGridPosition();

        if (previewActive && type != HoverType.Tile) {
            tm.RemovePreview(previewPoint);
            previewActive = false;
        }

        // We are still hovering over a tile but the preview has moved; so remove the old and update it
        else if (previewActive && type == HoverType.Tile && previewPoint != newPreviewPoint) {
            tm.RemovePreview(previewPoint);

            previewPoint = newPreviewPoint;
            SetDestroyPreview();
        }

        // No active selection, but we need one
        else if (!previewActive && type == HoverType.Tile) {
            previewPoint = newPreviewPoint;
            SetDestroyPreview();
        }
    }

    void SetDestroyPreview() {
        (Vector2Int startPos, Constructable oldConstructable) = tm.GetConstructableAt(previewPoint);
        // We may not actually have anything here at all!
        if (oldConstructable == null) {
            previewActive = false;
            return;
        }

        previewActive = true;
        // Constructable newConstructable = CreateDestroyPreviewConstructable(oldConstructable);
        // tm.SetPreview(startPos, newConstructable);
    }

    // Constructable CreateDestroyPreviewConstructable(Constructable oldConstructable) {
    //     Constructable newConstructable = (Constructable) ScriptableObject.CreateInstance(typeof(Constructable));

    //     GridRow[] rows = new GridRow[oldConstructable.RowCount()];

    //     for (int row = 0 ; row < rows.Length ; row += 1) {
    //         GridEntry[] originalRow = oldConstructable.GetRow(row).gridEntries;

    //         GridEntry[] newRow = new GridEntry[originalRow.Length];

    //         for (int col = 0 ; col < newRow.Length ; col += 1) {
    //             if (originalRow[col].worldTile == null) {
    //                 newRow[col] = new GridEntry { worldTile = null, previewTile = null, obstructive = false };
    //             } else {
    //                 newRow[col] = new GridEntry { worldTile = previewTile, previewTile = previewTile, obstructive = false };
    //             }
    //         }

    //         rows[row] = new GridRow { gridEntries = newRow };
    //     }

    //     newConstructable.SetData(rows);

    //     return newConstructable;
    // }

    void Destroy(HoverData data) {
        if (data.GetHoverType() != HoverType.Tile) return;

        tm.Destroy(previewPoint);
        if (previewActive) {
            tm.RemovePreview(previewPoint);
            previewActive = false;
        }
    }

    public override void OnDequip() {
        if (previewActive) tm.RemovePreview(previewPoint);
        previewActive = false;
    }

}
