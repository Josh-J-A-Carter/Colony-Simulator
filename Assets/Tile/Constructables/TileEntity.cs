using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class TileEntity : Constructable {

    /// <summary>
    /// Pass a tile-entity instance's data to its parent constructable, in order to execute specific behaviour.
    /// <br></br><br></br>
    /// For non tile-entities, this function simply returns - but tile entities are derived from classes that 
    /// inherit from Constructable, allowing them to extend this function.
    /// </summary>
    public virtual void TickInstance(Vector2Int position, Dictionary<String, object> instance) {}

    public virtual Dictionary<String, object> GenerateDefaultData() {
        return new Dictionary<String, object>();
    }

    public void DrawVariant(Vector2Int position, Func<Vector2Int, TileBase> variantGenerator) {
        TileManager.Instance.DrawVariant(position, this, variantGenerator);
    }

    public virtual InfoBranch GetTileEntityInfoTree(Dictionary<String, object> instance) {
        InfoBranch root = new InfoBranch(String.Empty);

        if (this is Storage storage) {
            InfoBranch inventoryCategory = storage.GetInventory(instance).GetInfoTree();
            root.AddChild(inventoryCategory);
        }
        
        return root;
    }
}
