using System;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public static CameraManager Instance { get; private set; }


    // Scene & camera specifics
    GameObject root;
    Camera mainCamera;
    float cameraSizeDefault;

    BoundsInt maxBounds;

    // Pan speed fields & calculations
    static readonly float panSpeedBase = 0.75f;
    float UniDirectionalPanSpeed => panSpeedBase * zoom;
    // Let a, b be the velocities for x-, y-directions, let c be the velocity in the xy-direction.
    // We know that the xy-direction should have the same velocity as the unidirectional speed.
    // By Pythagoras, a^2 + b^2 = c^2
    // The velocities in x, y directions are the same, so
    // a^2 = b^2     =>    2 * a^2 = c^2
    //               =>    a = sqrt(c^2 / 2)
    float BiDirectionalPanSpeed => (float) (zoom * panSpeedBase / Math.Sqrt(2));


    // Input fields
    enum VerticalInput { None = 0, Up = 1, Down = -1 };
    enum HorizontalInput { None = 0, Right = 1, Left = -1 };
    HorizontalInput horizontalInput;
    VerticalInput verticalInput;

    // Minimum/maximum zoom levels allowed; default zoom level; how quickly to ease the zoom level
    static readonly float zoomMin = 0.5f, zoomMax = 2f, zoomDefault = 1.0f, zoomEaseFactorBase = 0.2f;
    // When zoom <= 1, change zoom by by 0.2f. When zoom > 1, this factor is too small, so increase linearly with zoom.
    static readonly Func<float, float> zoomEaseFactor = zoom => zoom <= 1 ? zoomEaseFactorBase : zoomEaseFactorBase * zoom;
    float zoom = zoomDefault;

    void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        root = gameObject;

        mainCamera = Camera.main;
        cameraSizeDefault = mainCamera.orthographicSize;
    }

    public void Start() {
        BoundsInt worldBounds = TileManager.Instance.GetBounds();

        Vector3Int boundsPadding = new(2, 2, 0);

        maxBounds = new(worldBounds.min - boundsPadding, worldBounds.size + 2 * boundsPadding);
    }

    public Bounds Bounds() {
        float screenAspect = Screen.width / (float) Screen.height;
        float cameraHeight = mainCamera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            mainCamera.transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }

    public void Run(HoverData data) {
        CheckInput(data);
    }

    public void FixedRun() {
        ApplyCameraChanges();
    }

    void CheckInput(HoverData data) {
        bool hoverUI = data.GetHoverType() == HoverType.UI;

        if (Input.GetAxis("Horizontal") < 0) horizontalInput = HorizontalInput.Left;
        else if (Input.GetAxis("Horizontal") > 0) horizontalInput = HorizontalInput.Right;
        else horizontalInput = HorizontalInput.None;

        if (Input.GetAxis("Vertical") < 0) verticalInput = VerticalInput.Down;
        else if (Input.GetAxis("Vertical") > 0) verticalInput = VerticalInput.Up;
        else verticalInput = VerticalInput.None;

        // Zoom
        // Do not enable zoom when hovering over UI!
        if (hoverUI) return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta < 0 && zoom < zoomMax) zoom = Math.Min(zoomMax, zoom - scrollDelta * zoomEaseFactor(zoom));
        else if (scrollDelta > 0 && zoom > zoomMin) zoom = Math.Max(zoomMin, zoom - scrollDelta * zoomEaseFactor(zoom));

        if (scrollDelta != 0) mainCamera.orthographicSize = cameraSizeDefault * zoom;
    }

    void ApplyCameraChanges() {
        // Determine the direction
        float hComp = (int) horizontalInput;
        float vComp = (int) verticalInput;

        Bounds cBounds = Bounds();

        if (hComp < 0 && cBounds.min.x <= maxBounds.min.x) hComp = 0;
        if (hComp > 0 && cBounds.max.x >= maxBounds.max.x) hComp = 0;

        if (vComp < 0 && cBounds.min.y <= maxBounds.min.y) vComp = 0;
        if (vComp > 0 && cBounds.max.y >= maxBounds.max.y) vComp = 0;

        Vector3 translation = new Vector3(hComp, vComp);

        // Determine the magnitude in each direction
        if (horizontalInput != HorizontalInput.None && verticalInput != VerticalInput.None) translation *= BiDirectionalPanSpeed;
        else translation *= UniDirectionalPanSpeed;

        // Move
        root.transform.Translate(translation);
    }
}
