using System;
using UnityEngine;

public class DestroyTool : Tool {

    [SerializeField]
    Constructable destroyEffect;
    TileManager tm => TileManager.Instance;

    // Single tile preview / cursor
    bool previewCursorActive = false;
    Vector2Int previewCursorPosition;

    // Selection area / preview
    bool previewAreaActive = false;
    Vector2Int startPreviewArea, endPreviewArea;
    const int MAX_SELECTION_AREA = 512;


    public override void Run(HoverData data) {
        SingleSelection(data);

        // Began selection by holding shift & pressing left click
        if (Input.GetKey(KeyCode.LeftShift) || previewAreaActive) { 
            AreaSelection(data);
        }
    }

    void SingleSelection(HoverData data) {
        HoverType type = data.GetHoverType();

        // If preview isn't active and we aren't hovering over a tile, don't start selection.
        if (!previewCursorActive && (type == HoverType.UI || type == HoverType.None)) return;

        // If we have an area preview, don't start the single selection preview.
        if (previewAreaActive) return;

        // If we have moved out of screen, onto UI, or are about to start area selection, delete the single selection
        if (previewCursorActive && (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Mouse0)
                                    || type == HoverType.UI || type == HoverType.None)) {
            previewCursorActive = false;
            tm.RemovePreview(previewCursorPosition);
            return;
        }

        Vector2Int newPos = data.GetGridPosition();

        // If we have moved the cursor & are hovering over a new tile, set that tile instead
        if (previewCursorActive && newPos != previewCursorPosition) {
            tm.RemovePreview(previewCursorPosition);
            previewCursorActive = false;
        }

        // Can only select destructable things
        (_, Constructable constructable) = tm.GetConstructableAt(newPos);
        if (constructable == null || !constructable.HasTag(ConstructableTag.HoneyBeeDestructable)) return;

        if (!previewCursorActive || newPos != previewCursorPosition) {
            tm.SetPreview(newPos, destroyEffect);
            previewCursorActive = true;
            previewCursorPosition = newPos;
        }

        // Build when clicked
        if (previewCursorActive && Input.GetKeyDown(KeyCode.Mouse0)) {
            BuildArea(previewCursorPosition, previewCursorPosition, destroyEffect);
        }
    }

    void AreaSelection(HoverData data) {
        HoverType type = data.GetHoverType();
        Vector2Int newStartPreview = data.GetGridPosition();
        Vector2Int newEndPreview = newStartPreview;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            // Remove existing preview
            if (previewAreaActive) RemovePreviewArea(startPreviewArea, endPreviewArea);

            // Set startPreview, endPreview to this & show preview
            startPreviewArea = newStartPreview;
            endPreviewArea = newEndPreview;

            SetPreviewArea(startPreviewArea, endPreviewArea, destroyEffect);
            previewAreaActive = true;
        }

        // Changed selection area, and new area is not too large
        else if (previewAreaActive && newEndPreview != endPreviewArea) {
            (Vector2Int p1, Vector2Int p2) = GetBounds(startPreviewArea, newEndPreview);
            if ((p2.x - p1.x) * (p1.y - p2.y) > MAX_SELECTION_AREA) {
                newEndPreview = GetClosestValidEndPreview(startPreviewArea, newEndPreview);
            }

            RemovePreviewArea(startPreviewArea, endPreviewArea);

            endPreviewArea = newEndPreview;

            SetPreviewArea(startPreviewArea, endPreviewArea, destroyEffect);
        }

        // Ended selection by releasing either shift or left click
        if (previewAreaActive && (!Input.GetKey(KeyCode.LeftShift) || !Input.GetKey(KeyCode.Mouse0))) {
            RemovePreviewArea(startPreviewArea, endPreviewArea);

            BuildArea(startPreviewArea, endPreviewArea, destroyEffect);

            previewAreaActive = false;
        }
    }

    Vector2Int GetClosestValidEndPreview(Vector2Int origin, Vector2Int extremity) {
        int maxIterations = 10;

        float minScale = 0.5f;
        float maxScale = 1.0f;

        float increment = (maxScale - minScale) / maxIterations;

        Vector2Int newPoint = Vector2Int.zero;

        for (int i = 1 ; i <= maxIterations ; i += 1) {
            float scale = maxScale - increment * i;

            newPoint = new Vector2Int(Mathf.FloorToInt(extremity.x * scale), Mathf.FloorToInt(extremity.y * scale));
            (Vector2Int p1, Vector2Int p2) = GetBounds(startPreviewArea, newPoint);
            if ((p2.x - p1.x) * (p1.y - p2.y) <= MAX_SELECTION_AREA) return newPoint;
        }

        return newPoint;
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

    void BuildArea(Vector2Int start, Vector2Int end, Constructable preview) {
        (Vector2Int p1, Vector2Int p2) = GetBounds(start, end);

        for (int x = p1.x ; x <= p2.x ; x += 1) {
            for (int y = p1.y ; y >= p2.y ; y -= 1) {
                Vector2Int pos = new(x, y);

                // Can only select destructable things
                (_, Constructable constructable) = tm.GetConstructableAt(pos);
                if (constructable == null || !constructable.HasTag(ConstructableTag.HoneyBeeDestructable)) return;

                TaskManager.Instance.CreateTask(new DestroyTask(parent.GetPriority(), pos, preview, constructable));
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

    InfoBranch GetConstructableConfigInfo(Constructable newConstructable) {
        InfoBranch root = new(String.Empty);

        // Generic
        InfoBranch genericCategory = new("Generic properties");
        root.AddChild(genericCategory);

        InfoLeaf nameProperty = new(newConstructable.GetName(), description: newConstructable.GetDescription());
        genericCategory.AddChild(nameProperty);

        // Build reqs
        InfoBranch reqsCategory = new("Required resources");
        root.AddChild(reqsCategory);

        foreach ((Resource res, uint quantity) in newConstructable.GetRequiredResources()) {
            String reqName;
            if (res.ResourceType == ResourceType.Item) reqName = res.Item.GetName();
            else reqName = res.ItemTag.GetDescription();

            InfoLeaf reqProperty = new(reqName, $"x {quantity}");
            reqsCategory.AddChild(reqProperty);
        }

        return root;
    }
}
