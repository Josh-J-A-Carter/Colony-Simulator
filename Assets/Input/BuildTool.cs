using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildTool : Tool {

    [SerializeField]
    NavNode navTreeRoot;

    Constructable constructable;

    TileManager tm => TileManager.Instance;

    Vector2Int previewPoint;
    bool previewActive = false;

    public override void Run(HoverData data) {
        Constructable newConstructable = parent.GetConstructable();
        if (newConstructable == null) return;

        Preview(data);

        if (Input.GetKeyDown(KeyCode.Mouse0)) Build(data);

        constructable = newConstructable;
    }

    public override void OnEquip() {
        NavToUI.DisplayNavTree(navTreeRoot, parent.SetConstructable);

        InterfaceManager.Instance.ShowConfigurableContainer();
    }

    void Preview(HoverData data) {
        HoverType type = data.GetType();

        Constructable newConstructable = parent.GetConstructable();
        Vector2Int newPreviewPoint = data.GetTileData();

        if (previewActive && type != HoverType.Tile) {
            tm.RemovePreview(previewPoint);
            previewActive = false;
        }

        // We are still hovering over a tile but the preview has moved, or the constructable has since changed
        else if (previewActive && type == HoverType.Tile && (previewPoint != newPreviewPoint || constructable != newConstructable)) {
            tm.RemovePreview(previewPoint);

            previewPoint = newPreviewPoint;
            constructable = newConstructable;
            tm.SetPreview(previewPoint, constructable);
        }

        // No active selection, but we need one
        else if (!previewActive && type == HoverType.Tile) {
            previewPoint = newPreviewPoint;
            constructable = newConstructable;
            previewActive = true;
            
            tm.SetPreview(previewPoint, constructable);
        }
    }

    void Build(HoverData data) {
        if (data.GetType() != HoverType.Tile) return;

        TaskManager.Instance.CreateTask(new BuildTask(TaskPriority.Normal, previewPoint, constructable));

        // tm.Construct(previewPoint, constructable);
    }

    public override void OnDequip() {
        if (previewActive) {
            tm.RemovePreview(previewPoint);
            previewActive = false;
        }

        InterfaceManager.Instance.HideConfigurableContainer();
    }

}
