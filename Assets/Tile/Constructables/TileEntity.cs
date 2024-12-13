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

    public abstract Dictionary<String, object> GenerateDefaultData();

    // public void Create(Vector2Int pos, Dictionary<String, object> instance) {
    //     if (this is Storage storage) storage.OnStorageCreation(pos, instance);

    //     OnCreation(pos, instance);
    // }

    // public void Destroy(Vector2Int pos, Dictionary<String, object> instance) {
    //     if (this is Storage storage) storage.OnStorageDestruction(pos, instance);

    //     OnDestruction(pos, instance);
    // }

    // protected virtual void OnCreation(Vector2Int pos, Dictionary<String, object> instance) {}

    // protected virtual void OnDestruction(Vector2Int pos, Dictionary<String, object> instance) {}

    public void DrawVariant(Vector2Int position, Func<Vector2Int, TileBase> variantGenerator) {
        TileManager.Instance.DrawVariant(position, this, variantGenerator);
    }

    public virtual InfoBranch GetTileEntityInfoTree(Dictionary<String, object> instance) {
        InfoBranch root = new InfoBranch(String.Empty);
                
        return root;
    }
}
