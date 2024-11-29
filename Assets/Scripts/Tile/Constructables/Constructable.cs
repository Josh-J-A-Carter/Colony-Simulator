using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Constructable")]
public class Constructable : ScriptableObject {

    [SerializeField]
    GridRow[] gridData;

    public bool isTileEntity { get; protected set; } = false;

    public TileEntityData defaultData { get; protected set; } = null;

    public int RowCount() {
        return gridData.Length;
    }

    public GridRow GetRow(int index) {
        if (index < 0 || index >= RowCount()) throw new Exception($"No row with index {index} in this Constructable, {this}.");

        return gridData[index];
    }

    public void SetData(GridRow[] gridData) {
        this.gridData = gridData;
    }

    /// <summary>
    /// Pass a tile-entity instance's data to its parent constructable, in order to execute specific behaviour.
    /// <br></br><br></br>
    /// For non tile-entities, this function simply returns - but tile entities are derived from classes that 
    /// inherit from Constructable, allowing them to extend this function.
    /// </summary>
    public virtual void TickTileEntity(TileEntityData instance) {}
}

[Serializable]
public struct GridRow {
    public GridEntry[] gridEntries;
}

[Serializable]
public struct GridEntry {
    public TileBase worldTile;
    public TileBase previewTile;
    public bool obstructive;
}
