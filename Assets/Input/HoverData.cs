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
    Vector2Int gridPos;

    public HoverData(VisualElement UIData, Vector2Int gridPos) {
        this.type = HoverType.UI;
        this.UIData = UIData;

        this.gridPos = gridPos;
    }

    public HoverData(GameObject entity, Vector2Int gridPos) {
        this.type = HoverType.Entity;
        this.entityData = entity;

        this.gridPos = gridPos;
    }

    public HoverData(Vector2Int gridPos) {
        this.type = HoverType.Tile;
        
        this.gridPos = gridPos;
    }

    public HoverData() {
        this.type = HoverType.None;
    }

    public HoverType GetHoverType() {
        return this.type;
    }

    public VisualElement GetUIData() {
        return this.UIData;
    }

    public GameObject GetEntityData() {
        return this.entityData;
    }

    public Vector2Int GetGridPosition() {
        return this.gridPos;
    }
}
