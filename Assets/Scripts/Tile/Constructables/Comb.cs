using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Comb")]
public class Comb : Constructable {

    void Awake() {
        isTileEntity = true;

        defaultData = new TileEntityData(new (int, int)[2] {
            ((int) CombAttr.ContainsBrood, 1), ((int) CombAttr.ContainsFor, 0)
        });
    }

    /// <summary>
    /// Pass a tile-entity instance's data to its parent constructable, in order to execute specific behaviour.
    /// <br></br><br></br>
    /// For non tile-entities, this function simply returns - but tile entities are derived from classes that 
    /// inherit from Constructable, allowing them to extend this function.
    /// </summary>
    public override void TickTileEntity(TileEntityData instance) {

        int containsBrood, containsFor;
        if (instance.TryGetAttribute((int) CombAttr.ContainsBrood, out containsBrood)
            && instance.TryGetAttribute((int) CombAttr.ContainsFor, out containsFor)) {
            
            if (containsFor >= 10) {
                instance.SetAttribute((int) CombAttr.ContainsBrood, 0);
                instance.SetAttribute((int) CombAttr.ContainsFor, 0);
                Debug.Log("yay adult now or something");
                TileManager.Instance.Destroy(new Vector2Int(-4, -3));
            }
            
            else instance.SetAttribute((int) CombAttr.ContainsFor, containsFor + 1);
        }
    }
}

public enum CombAttr {
    ContainsBrood,
    ContainsFor
}
