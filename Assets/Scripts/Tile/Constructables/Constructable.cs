using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Constructable")]
public class Constructable : ScriptableObject {

    [SerializeField]
    Row[] rows;

    public int RowCount() {
        return rows.Length;
    }

    public Row GetRow(int index) {
        if (index < 0 || index >= RowCount()) throw new Exception($"No row with index {index} in this Constructable, {this}.");

        return rows[index];
    }

    public void SetRows(Row[] rows) {
        this.rows = rows;
    }
}

[Serializable]
public struct Row {
    public TileConstruct[] tileConstructs;
}

[Serializable]
public struct TileConstruct {
    public TileBase worldTile;
    public TileBase previewTile;
    public bool obstructive;
}
