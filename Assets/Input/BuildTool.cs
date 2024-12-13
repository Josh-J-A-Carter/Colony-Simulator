using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildTool : Tool {

    [SerializeField]
    NavNode navTreeRoot;
    Constructable constructable;
    TileManager tm => TileManager.Instance;

    Vector2Int startPreview, endPreview;
    Dictionary<String, object> previewConfigDataTemplate;
    bool previewActive = false;

    const int MAX_SELECTION = 512;

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
            ShowInfoContainers();
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

        // Changed selection area, and new area is not too large
        else if (previewActive && newEndPreview != endPreview) {
            (Vector2Int p1, Vector2Int p2) = GetBounds(startPreview, newEndPreview);
            if ((p2.x - p1.x) * (p1.y - p2.y) > MAX_SELECTION) {
                newEndPreview = GetClosestValidEndPreview(startPreview, newEndPreview);
            }

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

    Vector2Int GetClosestValidEndPreview(Vector2Int origin, Vector2Int extremity) {
        int maxIterations = 10;

        float minScale = 0.5f;
        float maxScale = 1.0f;

        float increment = (maxScale - minScale) / maxIterations;

        Vector2Int newPoint = Vector2Int.zero;

        for (int i = 1 ; i <= maxIterations ; i += 1) {
            float scale = maxScale - increment * i;

            newPoint = new Vector2Int(Mathf.FloorToInt(extremity.x * scale), Mathf.FloorToInt(extremity.y * scale));
            (Vector2Int p1, Vector2Int p2) = GetBounds(startPreview, newPoint);
            if ((p2.x - p1.x) * (p1.y - p2.y) <= MAX_SELECTION) return newPoint;
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
                TaskManager.Instance.CreateTask(new BuildTask(TaskPriority.Normal, new Vector2Int(x, y), constructable, previewConfigDataTemplate));
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


        if (constructable is Configurable configurable) {
            previewConfigDataTemplate = (constructable as TileEntity).GenerateDefaultData();
            InfoBranch configurableProperties = configurable.GetConfigTree(previewConfigDataTemplate);
            root.AddChild(configurableProperties);
        }

        return root;
    }

    void ShowInfoContainers() {
        InfoBranch configInfo = GetConstructableConfigInfo();

        Action<String[], bool> callback = null;
        if (constructable is Configurable) callback = OnConfigUpdate;
        InfoToUI.DisplayConfigInfoTree(configInfo, callback);
        InterfaceManager.Instance.ShowConfigInfoContainer();
    }

    public override void OnEquip() {
        NavToUI.DisplayNavTree(navTreeRoot, parent.SetConstructable);

        InterfaceManager.Instance.ShowConfigurableContainer();

        if (constructable) ShowInfoContainers();
    }

    public override void OnDequip() {
        InterfaceManager.Instance.HideConfigurableContainer();

        InterfaceManager.Instance.HideConfigInfoContainer();
    }

}
