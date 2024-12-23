using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour {

    public static InputManager Instance { get; private set; }
    Camera mainCamera;

    CameraManager cameraManager;

    // Tool references
    Tool selectTool, buildTool, destroyTool, forageTool, cancelTool;
    Tool currentTool, previousTool;

    // Tool selections that should be persistent, even across tool changes
    Constructable currentConstructable;

    TaskPriority currentPriority;

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        // Tool
        currentPriority = TaskPriority.Normal;

        selectTool = GetComponentInChildren<SelectTool>();
        buildTool = GetComponentInChildren<BuildTool>();
        destroyTool = GetComponentInChildren<DestroyTool>();
        forageTool = GetComponentInChildren<ForageTool>();
        cancelTool = GetComponentInChildren<CancelTool>();

        selectTool.SetUp(this);
        buildTool.SetUp(this);
        destroyTool.SetUp(this);
        cancelTool.SetUp(this);
        forageTool.SetUp(this);

        currentTool = selectTool;
        previousTool = selectTool;

        // Camera
        cameraManager = GetComponentInChildren<CameraManager>();
        mainCamera = Camera.main;
    }

    public void Start() {
        currentTool?.OnEquip();
    }

    public void Update() {
        // calculate hover data
        HoverData hoverData = GenerateHoverData();
        
        currentTool?.Run(hoverData);

        cameraManager?.Run(hoverData);
    }

    public void FixedUpdate() {
        currentTool?.FixedRun();
        
        cameraManager?.FixedRun();
    }

    HoverData GenerateHoverData() {
        Vector2 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));

        // 1. Check whether UI is blocking the mouse
        if (EventSystem.current.IsPointerOverGameObject()) {
            return new HoverData((VisualElement) null, gridPos);

            /// 
            ///  NOTE: Need to use UIData specific HoverData!
            ///
        }

        // 2. Look for entities (i.e. game objects with collider components)
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit.collider != null) return new HoverData(hit.collider.gameObject, gridPos);

        // 3. We have no UI or entities here, but the cursor is in the window. Thus, select the appropriate tile position
        if (mainCamera.pixelRect.Contains(Input.mousePosition)) return new HoverData(gridPos);

        // 4. The cursor is not even inside the window; nothing is selected.
        return new HoverData();
    }

    public void SetTool(ToolType type) {

        Tool newTool = type switch {
            ToolType.Select => selectTool,
            ToolType.Build => buildTool,
            ToolType.Destroy => destroyTool,
            ToolType.Forage => forageTool,
            ToolType.Cancel => cancelTool,
            _ => throw new Exception("Unknown tool type")
        };

        if (newTool == currentTool) return;

        previousTool = currentTool;
        currentTool = newTool;

        previousTool.OnDequip();
        currentTool.OnEquip();
    }

    public void RestorePreviousTool() {
        if (currentTool == previousTool) return;

        currentTool.OnDequip();
        currentTool = previousTool;
        currentTool.OnEquip();
    }

    public Constructable GetConstructable() {
        return currentConstructable;
    }

    public void SetConstructable(Constructable constructable) {
        currentConstructable = constructable;
    }

    public TaskPriority GetPriority() {
        return currentPriority;
    }

    public void SetPriority(TaskPriority priority) {
        currentPriority = priority;
    }
}

public enum ToolType {
    Select,
    Build,
    Destroy,
    Forage,
    Cancel
}