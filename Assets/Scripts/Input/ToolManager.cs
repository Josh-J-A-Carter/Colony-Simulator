using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ToolManager : MonoBehaviour {

    public static ToolManager Instance { get; private set; }
    Camera camera;

    // Tool references
    Tool selectTool, buildTool, destroyTool;
    Tool currentTool;

    // Tool selections that should be persistent, even across tool changes
    [SerializeField]
    public Constructable currentConstructable;


    void Awake() {
        // Instantiate singleton
        if (Instance == null) Instance = (ToolManager) this;
        else if (Instance != this) {
            Destroy(this);
            return;
        }

        Instantiate();
    }

    public void Instantiate() {
        camera = Camera.main;

        selectTool = GetComponentInChildren<SelectTool>();
        buildTool = GetComponentInChildren<BuildTool>();
        destroyTool = GetComponentInChildren<DestroyTool>();
        selectTool.SetUp(this);
        buildTool.SetUp(this);
        destroyTool.SetUp(this);

        currentTool = selectTool;
    }

    public void Update() {
        // calculate hover data
        HoverData hoverData = GenerateHoverData();
        
        currentTool?.Run(hoverData);
    }

    HoverData GenerateHoverData() {
        // 1. Check whether UI is blocking the mouse
        if (EventSystem.current.IsPointerOverGameObject()) {
            return new HoverData((VisualElement) null);

            ///
            /// 
            /// 
            ///  NOTE: Need to use UIData specific HoverData!
            /// 
            ///
        }

        // 2. Look for entities (i.e. game objects with collider components)
        Vector2 worldMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldMousePosition, Vector2.zero);

        if (hit.collider != null) return new HoverData(hit.collider.gameObject);

        // 3. We have no UI or entities here, but the cursor is in the window. Thus, select the appropriate tile position
        if (camera.pixelRect.Contains(Input.mousePosition)) {
            Vector2 pos = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int tileData = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));
            return new HoverData(tileData);
        }

        // 4. The cursor is not even inside the window; nothing is selected.
        return new HoverData();
    }

    public void SetTool(ToolType type) {

        Tool oldTool = currentTool;

        switch (type) {
            case ToolType.Select:
                currentTool = selectTool;
                break;
            case ToolType.Build:
                currentTool = buildTool;
                break;
            case ToolType.Destroy:
                currentTool = destroyTool;
                break;
        }

        if (oldTool != currentTool) {
            oldTool.OnDequip();
            currentTool.OnEquip();
        }
    }
}

public enum ToolType {
    Select,
    Build,
    Destroy
}