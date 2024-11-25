using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum HoverType { UI, Entity, Tile, None }

public class HoverData {
    HoverType type;

    // UI Data
    VisualElement UIData;

    // Entity Data
    GameObject entityData;

    // Tile Data
    Vector2Int tileData;

    public HoverData(VisualElement UIData) {
        this.UIData = UIData;
    }

    public HoverData(GameObject entity) {
        this.type = HoverType.Entity;
        this.entityData = entity;
    }

    public HoverData(Vector2Int tileData) {
        this.tileData = tileData;
    }

    public HoverData() {
        this.type = HoverType.None;
    }

    public HoverType GetType() {
        return this.type;
    }

    public VisualElement GetUIData() {
        return this.UIData;
    }

    public GameObject GetEntityData() {
        return this.entityData;
    }

    public Vector2Int GetTileData() {
        return this.tileData;
    }
}
