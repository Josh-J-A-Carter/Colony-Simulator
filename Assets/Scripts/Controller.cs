using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{

    static readonly float panSpeedBase = 0.75f;
    float UniDirectionalPanSpeed => panSpeedBase * zoom;
    // Let a, b be the velocities for x-, y-directions, let c be the velocity in the xy-direction.
    // We know that the xy-direction should have the same velocity as the unidirectional speed.
    // By Pythagoras, a^2 + b^2 = c^2
    // The velocities in x, y directions are the same, so
    // a^2 = b^2     =>    2 * a^2 = c^2
    //               =>    a = sqrt(c^2 / 2)
    float BiDirectionalPanSpeed => (float) (zoom * panSpeedBase / Math.Sqrt(2));

    enum VertDir { None = 0, Up = 1, Down = -1 };
    enum HorizDir { None = 0, Right = 1, Left = -1 };

    (VertDir, HorizDir) holdingKeys = (VertDir.None, HorizDir.None);

    static readonly float zoomMin = 0.5f;
    static readonly float zoomMax = 5f;
    static readonly float zoomDefault = 1.0f;
    static readonly float zoomEaseFactorBase = 0.2f;
    // When zoom <= 1, change zoom by by 0.2f. When zoom > 1, this factor is too small, so increase linearly with zoom.
    static readonly Func<float, float> zoomEaseFactor = zoom => zoom <= 1 ? zoomEaseFactorBase : zoomEaseFactorBase * zoom;
    float zoom = zoomDefault;

    Camera camera;

    float cameraSizeDefault;


    // Tool Selection
    [SerializeField]
    Tilemap previewMap, gameWorld, obstacles;
    
    [SerializeField]
    Tile selectPreviewTile, constructPreviewTile, constructTile, destroyPreviewTile, obstacle;
    Tile destroyTile = null;

    Vector2Int previewPoint;

    bool previewPointActive = false;

    public enum Tool {
        Select,
        Construct,
        Destroy
    }

    public static Tool toolSelection = Tool.Select;



    // Mouse Selection

    enum MouseSelection {
        UI,
        Entity,
        Tile,
        None
    }

    MouseSelection mouseSelection;
    GameObject entitySelection;
    Vector2Int tileSelection;


    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        cameraSizeDefault = camera.orthographicSize;
    }

    void Update() {
        CheckButtonInput();

        CheckMouseSelection();

        
        ////// Very sloppily giving a preview of the current tool

        // We are no longer hovering over a tile, but the preview is still active; so remove it
        if (previewPointActive && mouseSelection != MouseSelection.Tile) {
            previewMap.SetTile(new Vector3Int(previewPoint.x, previewPoint.y, 0), null);
            previewPointActive = false;
        }
        // We are still hovering over a tile but the preview has moved; so remove the old and update it
        else if (previewPointActive && mouseSelection == MouseSelection.Tile && previewPoint != tileSelection) {
            previewMap.SetTile(new Vector3Int(previewPoint.x, previewPoint.y, 0), null);

            Tile tile = selectPreviewTile;
            if (toolSelection == Tool.Construct) tile = constructPreviewTile;
            else if (toolSelection == Tool.Destroy) tile = destroyPreviewTile;
            
            previewPoint = tileSelection;
            previewMap.SetTile(new Vector3Int(previewPoint.x, previewPoint.y, 0), tile);
        }
        else if (!previewPointActive && mouseSelection == MouseSelection.Tile) {
            previewPointActive = true;

            Tile tile = selectPreviewTile;
            if (toolSelection == Tool.Construct) tile = constructPreviewTile;
            else if (toolSelection == Tool.Destroy) tile = destroyPreviewTile;
            
            previewPoint = tileSelection;
            previewMap.SetTile(new Vector3Int(previewPoint.x, previewPoint.y, 0), tile);
        }


        ////// Very sloppily using the tool
        if (Input.GetKeyDown(KeyCode.Mouse0) && mouseSelection == MouseSelection.Tile) {
            if (toolSelection == Tool.Construct) {
                gameWorld.SetTile(new Vector3Int(tileSelection.x, tileSelection.y, 0), constructTile);
                obstacles.SetTile(new Vector3Int(tileSelection.x, tileSelection.y, 0), obstacle);
            } else if (toolSelection == Tool.Destroy) {
                gameWorld.SetTile(new Vector3Int(tileSelection.x, tileSelection.y, 0), destroyTile);
                obstacles.SetTile(new Vector3Int(tileSelection.x, tileSelection.y, 0), destroyTile);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyButtonMotion();

        // Debug.Log(mouseSelection);
    }

    void CheckButtonInput() {
        // Note: holdingKeys is structured as (verticalDirection, horizontalDirection)

        // Check if any keys have been released
        if (Input.GetKeyUp(KeyCode.W) && holdingKeys.Item1 == VertDir.Up) holdingKeys.Item1 = VertDir.None;
        if (Input.GetKeyUp(KeyCode.S) && holdingKeys.Item1 == VertDir.Down) holdingKeys.Item1 = VertDir.None;
        if (Input.GetKeyUp(KeyCode.A) && holdingKeys.Item2 == HorizDir.Left) holdingKeys.Item2 = HorizDir.None;
        if (Input.GetKeyUp(KeyCode.D) && holdingKeys.Item2 == HorizDir.Right) holdingKeys.Item2 = HorizDir.None;
        
        // Check if new keys have been pressed
        if (Input.GetKeyDown(KeyCode.W)) {holdingKeys.Item1 = VertDir.Up;}
        if (Input.GetKeyDown(KeyCode.S)) holdingKeys.Item1 = VertDir.Down;
        if (Input.GetKeyDown(KeyCode.A)) holdingKeys.Item2 = HorizDir.Left;
        if (Input.GetKeyDown(KeyCode.D)) holdingKeys.Item2 = HorizDir.Right;

        // Zoom
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta < 0 && zoom < zoomMax) zoom = Math.Min(zoomMax, zoom - scrollDelta * zoomEaseFactor(zoom));
        else if (scrollDelta > 0 && zoom > zoomMin) zoom = Math.Max(zoomMin, zoom - scrollDelta * zoomEaseFactor(zoom));

        if (scrollDelta != 0) {
            camera.orthographicSize = cameraSizeDefault * zoom;
        }
    }

    void CheckMouseSelection() {

        // 1. Check whether UI is blocking the mouse
        if (EventSystem.current.IsPointerOverGameObject()) {
            mouseSelection = MouseSelection.UI;
            return;
        }

        // 2. Look for entities
        Vector2 worldMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldMousePosition, Vector2.zero);

        if (hit.collider != null) {
            mouseSelection = MouseSelection.Entity;
            entitySelection = hit.collider.gameObject;
            return;
        }

        // 3. We have no UI or entities here, but the cursor is in the window. Thus, select the appropriate tile position
        if (camera.pixelRect.Contains(Input.mousePosition)) {
            mouseSelection = MouseSelection.Tile;
            Vector2 pos = camera.ScreenToWorldPoint(Input.mousePosition);
            tileSelection = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));
            return;
        }

        // 4. The cursor is not even inside the window; nothing is selected.
        mouseSelection = MouseSelection.None;
    }

    void ApplyButtonMotion() {

        // Determine the direction
        Vector3 translation = new Vector3((int) holdingKeys.Item2, (int) holdingKeys.Item1);

        // Determine the magnitude in each direction
        if (holdingKeys.Item1 != VertDir.None && holdingKeys.Item2 != HorizDir.None) translation *= BiDirectionalPanSpeed;
        else translation *= UniDirectionalPanSpeed;

        // Move
        transform.Translate(translation);
    }
}
