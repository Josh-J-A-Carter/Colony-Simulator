using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour {

    // Camera controller
    CameraController cameraController = new CameraController();

    // Tool controller
    ToolController toolController = new ToolController();

    Camera camera;


    // Tool Selection
    [SerializeField]
    Tile selectPreviewTile, constructPreviewTile, constructTile, destroyPreviewTile, obstacle;
    Tile destroyTile = null;


    void Start() {
        camera = GetComponentInChildren<Camera>();

        cameraController.Setup(gameObject, camera);
        toolController.Setup(gameObject, camera);
    }

    void Update() {
        cameraController.Update();
        toolController.Update();
    }

    void FixedUpdate() {
        cameraController.FixedUpdate();
    }
}
