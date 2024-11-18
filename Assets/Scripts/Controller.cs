using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        cameraSizeDefault = camera.orthographicSize;
    }

    void Update() {
        checkButtonInput();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Debug.Log(holdingKeys);
        applyButtonMotion();
    }

    void checkButtonInput() {
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

    void applyButtonMotion() {

        // Determine the direction
        Vector3 translation = new Vector3((int) holdingKeys.Item2, (int) holdingKeys.Item1);

        // Determine the magnitude in each direction
        if (holdingKeys.Item1 != VertDir.None && holdingKeys.Item2 != HorizDir.None) translation *= BiDirectionalPanSpeed;
        else translation *= UniDirectionalPanSpeed;

        // Move
        transform.Translate(translation);
    }
}
