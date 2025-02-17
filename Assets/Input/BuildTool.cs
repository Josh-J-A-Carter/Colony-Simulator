using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildTool : Tool {

    [SerializeField]
    NavNode navTreeRoot;
    Constructable constructable;
    TileManager tm => TileManager.Instance;

    // Single tile preview / cursor
    bool previewCursorActive = false;
    Vector2Int previewCursorPosition;

    // Selection area / preview
    bool previewAreaActive = false;
    Vector2Int startPreviewArea, endPreviewArea;
    const int MAX_SELECTION_AREA = 512;


    Dictionary<String, object> previewConfigDataTemplate;


    public override void Run(HoverData data) {
        Constructable newConstructable = parent.GetConstructable();
                
        // New constructable is null; not useful
        if (newConstructable == null) return;

        // Old constructable is null or different to new one, so need to display
        if (constructable != newConstructable) {
            ShowInfoContainers(newConstructable);
        }

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

        if (!previewCursorActive || newPos != previewCursorPosition) {
            tm.SetPreview(newPos, constructable);
            previewCursorActive = true;
            previewCursorPosition = newPos;
        }

        // Build when clicked
        if (previewCursorActive && Input.GetKeyDown(KeyCode.Mouse0)) {
            BuildArea(previewCursorPosition, previewCursorPosition, constructable);
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

            SetPreviewArea(startPreviewArea, endPreviewArea, constructable);
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

            SetPreviewArea(startPreviewArea, endPreviewArea, constructable);
        }

        // Ended selection by releasing either shift or left click
        if (previewAreaActive && (!Input.GetKey(KeyCode.LeftShift) || !Input.GetKey(KeyCode.Mouse0))) {
            RemovePreviewArea(startPreviewArea, endPreviewArea);

            BuildArea(startPreviewArea, endPreviewArea, constructable);

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

    void OnConfigUpdate(String[] path, bool newValue) {
        Dictionary<String, object> subTree = previewConfigDataTemplate;
        for (int i = 0 ; i < path.Length - 1 ; i += 1) {
            String step = path[i];
            subTree = (Dictionary<String, object>) subTree[step];
        }

        subTree[path[path.Length - 1]] = newValue;
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
                TaskManager.Instance.CreateTask(new BuildTask(parent.GetPriority(), new Vector2Int(x, y), constructable, previewConfigDataTemplate));
            }
        }

        // IMPORTANT! Update after setting tasks, otherwise changing the configuration parameters
        // will change the tasks too... not desired
        if (previewConfigDataTemplate != null) previewConfigDataTemplate = Utilities.RecursiveDataCopy(previewConfigDataTemplate);
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

        // Configuration data, if applicable
        if (newConstructable is IConfigurable configurable) {
            // Only update preview data if it's a different constructable - reloading shouldn't change config
            if (newConstructable != constructable) previewConfigDataTemplate = (configurable as TileEntity).GenerateDefaultData();
            InfoBranch configurableProperties = configurable.GetConfigTree(previewConfigDataTemplate);
            root.AddChild(configurableProperties);
        }

        return root;
    }

    void ShowInfoContainers(Constructable newConstructable) {
        InfoBranch configInfo = GetConstructableConfigInfo(newConstructable);
        constructable = newConstructable;

        Action<String[], bool> callback = null;
        if (constructable is IConfigurable) callback = OnConfigUpdate;
        InfoToUI.DisplayConfigInfoTree(configInfo, callback);
        InterfaceManager.Instance.ShowConfigInfoContainer();
    }

    public override void OnEquip() {
        NavToUI.DisplayNavTree(navTreeRoot, parent.SetConstructable);

        InterfaceManager.Instance.ShowConfigurableContainer();

        if (constructable) ShowInfoContainers(constructable);
    }

    public override void OnDequip() {
        InterfaceManager.Instance.HideConfigurableContainer();

        InterfaceManager.Instance.HideConfigInfoContainer();
    }

}
