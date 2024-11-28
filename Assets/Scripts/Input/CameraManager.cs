using System;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public static CameraManager Instance { get; private set; }


    // Scene & camera specifics
    GameObject root;
    Camera mainCamera;
    float cameraSizeDefault;

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
    static readonly float zoomMin = 0.5f, zoomMax = 5f, zoomDefault = 1.0f, zoomEaseFactorBase = 0.2f;
    // When zoom <= 1, change zoom by by 0.2f. When zoom > 1, this factor is too small, so increase linearly with zoom.
    static readonly Func<float, float> zoomEaseFactor = zoom => zoom <= 1 ? zoomEaseFactorBase : zoomEaseFactorBase * zoom;
    float zoom = zoomDefault;

    void Awake() {
        // Instantiate singleton
        if (Instance == null) Instance = this;
        else if (Instance != this) {
            Destroy(this);
            return;
        }

        root = gameObject;

        mainCamera = Camera.main;
        cameraSizeDefault = mainCamera.orthographicSize;
    }

    public void Update() {
        CheckInput();
    }

    public void FixedUpdate() {
        ApplyCameraChanges();
    }

    void CheckInput() {
        if (Input.GetAxis("Horizontal") < 0) horizontalInput = HorizontalInput.Left;
        else if (Input.GetAxis("Horizontal") > 0) horizontalInput = HorizontalInput.Right;
        else horizontalInput = HorizontalInput.None;

        if (Input.GetAxis("Vertical") < 0) verticalInput = VerticalInput.Down;
        else if (Input.GetAxis("Vertical") > 0) verticalInput = VerticalInput.Up;
        else verticalInput = VerticalInput.None;

        // Zoom
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta < 0 && zoom < zoomMax) zoom = Math.Min(zoomMax, zoom - scrollDelta * zoomEaseFactor(zoom));
        else if (scrollDelta > 0 && zoom > zoomMin) zoom = Math.Max(zoomMin, zoom - scrollDelta * zoomEaseFactor(zoom));

        if (scrollDelta != 0) mainCamera.orthographicSize = cameraSizeDefault * zoom;
    }

    void ApplyCameraChanges() {
        // Determine the direction
        Vector3 translation = new Vector3((int) horizontalInput, (int) verticalInput);

        // Determine the magnitude in each direction
        if (horizontalInput != HorizontalInput.None && verticalInput != VerticalInput.None) translation *= BiDirectionalPanSpeed;
        else translation *= UniDirectionalPanSpeed;

        // Move
        root.transform.Translate(translation);
    }
}
