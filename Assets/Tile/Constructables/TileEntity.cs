using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void DrawVariant(Vector2Int position, GridRow[] variantData) {
        TileManager.Instance.DrawVariant(position, variantData);
    }

}
