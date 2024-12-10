using System;
using UnityEngine;

public class BuildTool : Tool {

    [SerializeField]
    NavNode navTreeRoot;
    Constructable constructable;
    TileManager tm => TileManager.Instance;

    Vector2Int startPreview, endPreview;
    bool previewActive = false;

    public override void Run(HoverData data) {
        Constructable newConstructable = parent.GetConstructable();
        
        HoverType type = data.GetHoverType();
        Vector2Int newStartPreview = data.GetGridPosition();
        Vector2Int newEndPreview = newStartPreview;
        
        // New constructable is null; not useful
        if (newConstructable == null) return;

        // Old constructable is null or different to new one, so need to display
        if (constructable != newConstructable) {
            constructable = newConstructable;
            InfoToUI.DisplayConfigInfoTree(GetConstructableConfigInfo());
            InterfaceManager.Instance.ShowConfigInfoContainer();
        }

        if (type == HoverType.None) return;

        // Began selection by holding shift & pressing left click
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Mouse0)) {
            // Remove existing preview
            if (previewActive) RemovePreviewArea(startPreview, endPreview);

            // Set startPreview, endPreview to this & show preview
            startPreview = newStartPreview;
            endPreview = newEndPreview;

            SetPreviewArea(startPreview, endPreview, constructable);
            previewActive = true;
        }
        else if (previewActive && newEndPreview != endPreview) {
            RemovePreviewArea(startPreview, endPreview);

            endPreview = newEndPreview;

            SetPreviewArea(startPreview, endPreview, constructable);
        }

        // Ended selection by releasing either shift or left click
        else if (previewActive && (!Input.GetKey(KeyCode.LeftShift) || !Input.GetKey(KeyCode.Mouse0))) {
            RemovePreviewArea(startPreview, endPreview);

            BuildArea(startPreview, endPreview, constructable);

            previewActive = false;
        }

        // If we haven't been using the area selection tool, but click on a single tile, then set that tile
        else if (!previewActive && Input.GetKeyUp(KeyCode.Mouse0) && type != HoverType.UI) {
            BuildArea(newStartPreview, newEndPreview, constructable);
        }
    }

    void RemovePreviewArea(Vector2Int start, Vector2Int end) {
        (Vector2Int p1, Vector2Int p2) = GetBounds(start, end);

        for (int x = p1.x ; x <= p2.x ; x += 1) {
            for (int y = p1.y ; y >= p2.y ; y -= 1) {
                tm.RemovePreview(new Vector2Int(x, y));
            }
        }
    }

    void SetPreviewArea(Vector2Int start, Vector2Int end, Constructable constructable) {
        (Vector2Int p1, Vector2Int p2) = GetBounds(start, end);

        for (int x = p1.x ; x <= p2.x ; x += 1) {
            for (int y = p1.y ; y >= p2.y ; y -= 1) {
                tm.SetPreview(new Vector2Int(x, y), constructable);
            }
        }
    }

    void BuildArea(Vector2Int start, Vector2Int end, Constructable constructable) {
        (Vector2Int p1, Vector2Int p2) = GetBounds(start, end);

        for (int x = p1.x ; x <= p2.x ; x += 1) {
            for (int y = p1.y ; y >= p2.y ; y -= 1) {
                TaskManager.Instance.CreateTask(new BuildTask(TaskPriority.Normal, new Vector2Int(x, y), constructable));
            }
        }
    }


    /// <summary>
    /// Given two starting points which form corners of a rectangle, find the 
    /// top-left corner, and bottom-right corner, returning them as a tuple in this order.
    /// </summary>
    (Vector2Int, Vector2Int) GetBounds(Vector2Int p1, Vector2Int p2) {
        // Top-left corner of region
        int startX = Math.Min(p1.x, p2.x);
        int startY = Math.Max(p1.y, p2.y);
        // Bottom-right corner of region
        int endX = Math.Max(p1.x, p2.x);
        int endY = Math.Min(p1.y, p2.y);

        return (new Vector2Int(startX, startY), new Vector2Int(endX, endY));
    }

    InfoBranch GetConstructableConfigInfo() {
        InfoBranch root = new InfoBranch(null);

        InfoBranch genericCategory = new InfoBranch("Generic properties");
        root.AddChild(genericCategory);

        InfoLeaf nameProperty = new InfoLeaf(constructable.GetName(), description: constructable.GetDescription());
        genericCategory.AddChild(nameProperty);

        return root;
    }

    public override void OnEquip() {
        NavToUI.DisplayNavTree(navTreeRoot, parent.SetConstructable);

        InterfaceManager.Instance.ShowConfigurableContainer();

        if (constructable) {
            InfoToUI.DisplayConfigInfoTree(GetConstructableConfigInfo());
            InterfaceManager.Instance.ShowConfigInfoContainer();
        }
    }

    public override void OnDequip() {
        InterfaceManager.Instance.HideConfigurableContainer();

        InterfaceManager.Instance.HideConfigInfoContainer();
    }

}
