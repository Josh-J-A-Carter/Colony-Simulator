using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ToolController {

    GameObject root;
    Camera camera;

    // Tool references
    Tool selectorTool = new SelectorTool();

    Tool buildTool = new BuildTool();

    Tool destroyTool = new DestroyTool();

    Tool currentTool;

    public void Setup(GameObject root, Camera camera) {
        this.root = root;
        this.camera = camera;
        this.currentTool = selectorTool;
    }

    public void Update() {
        // calculate hover data
        HoverData hoverData = new HoverData();
        
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

}
