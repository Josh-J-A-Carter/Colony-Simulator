using System;
using System.Collections.ObjectModel;
using UnityEngine;

public class CancelTool : Tool {

    [SerializeField]
    Constructable cancelEffect;

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
            TileManager.Instance.RemovePreview(previewCursorPosition);
            return;
        }

        Vector2Int newPos = data.GetGridPosition();

        // If we have moved the cursor & are hovering over a new tile, set that tile instead
        if (previewCursorActive && newPos != previewCursorPosition) {
            TileManager.Instance.RemovePreview(previewCursorPosition);
        }

        // Need an actual task here
        ReadOnlyCollection<Task> currentHoverTasks = TaskManager.Instance.GetTasksAt(newPos);

        if (currentHoverTasks.Count == 0) return;
        
        bool foundCancellableTask = false;
        foreach (Task task in currentHoverTasks) {
            if (task.IsRuleGenerated() == false) {
                foundCancellableTask = true;
                break;
            }
        }


        if (foundCancellableTask == false) return;

        if (!previewCursorActive || newPos != previewCursorPosition) {
            TileManager.Instance.SetPreview(newPos, cancelEffect);
            previewCursorActive = true;
            previewCursorPosition = newPos;
        }

        // Cancel tasks when clicked
        if (previewCursorActive && Input.GetKeyDown(KeyCode.Mouse0)) {
            foreach (Task task in currentHoverTasks) {
                if (task.IsRuleGenerated()) continue;
                TaskManager.Instance.CancelTask(task);
            }
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

            SetPreviewArea(startPreviewArea, endPreviewArea);
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

            SetPreviewArea(startPreviewArea, endPreviewArea);
        }

        // Ended selection by releasing either shift or left click
        else if (previewAreaActive && (!Input.GetKey(KeyCode.LeftShift) || !Input.GetKey(KeyCode.Mouse0))) {
            RemovePreviewArea(startPreviewArea, endPreviewArea);

            BuildArea(startPreviewArea, endPreviewArea);

            previewAreaActive = false;
        }

        // If we haven't been using the area selection tool, but click on a single tile, then set that tile
        else if (!previewAreaActive && Input.GetKeyUp(KeyCode.Mouse0) && type != HoverType.UI) {
            BuildArea(newStartPreview, newEndPreview);
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
                TileManager.Instance.RemovePreview(new(x, y));
            }
        }
    }

    void SetPreviewArea(Vector2Int start, Vector2Int end) {
        (Vector2Int p1, Vector2Int p2) = GetBounds(start, end);

        for (int x = p1.x ; x <= p2.x ; x += 1) {
            for (int y = p1.y ; y >= p2.y ; y -= 1) {
                TileManager.Instance.SetPreview(new(x, y), cancelEffect);
            }
        }
    }

    void BuildArea(Vector2Int start, Vector2Int end) {
        (Vector2Int p1, Vector2Int p2) = GetBounds(start, end);

        for (int x = p1.x ; x <= p2.x ; x += 1) {
            for (int y = p1.y ; y >= p2.y ; y -= 1) {
                ReadOnlyCollection<Task> currentHoverTasks = TaskManager.Instance.GetTasksAt(new(x, y));
                if (currentHoverTasks.Count == 0) continue;

                foreach (Task task in currentHoverTasks) {
                    if (task.IsRuleGenerated()) continue;
                    TaskManager.Instance.CancelTask(task);
                }
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
}
