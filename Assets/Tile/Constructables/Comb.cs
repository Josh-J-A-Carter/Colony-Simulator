using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Comb")]
public class Comb : TileEntity {

    // Constants for TileEntityData attribute names

    /// <summary>The nature of the contents stored in a comb tile. The corresponding value should be of type <c>StorageType</c>.</summary>
    public const String STORAGE_TYPE = "storageType";
    public enum StorageType { Brood, Fermentable, Item, Empty };
    /// <summary>Data relevant to brood stored in a comb tile. The corresponding value should be of type <c>Dictionary</c>.</summary>
    public const String BROOD_DATA = "broodData";
    /// <summary>Part of the <c>BROOD_DATA</c> attribute, storing the comb's category of brood, with value of type <c>BroodType</c>.</summary>
    public const String BROOD_TYPE = "broodType";
    public enum BroodType { Worker, Drone, Queen };
    /// <summary>Part of <c>BROOD_DATA</c> attribute. It counts down the remaining number of ticks until the brood becomes an adult.
    /// The attribute's value should be of type <c>int</c>.</summary>
    public const String BROOD_TIME_LEFT = "timeLeft";

    public override Dictionary<String, object> GenerateDefaultData() {
        Dictionary<string, object> data = new Dictionary<string, object>();

        data[STORAGE_TYPE] = StorageType.Empty;

        return data;
    }

    /// <summary>
    /// Pass a tile-entity instance's data to its parent constructable, in order to execute specific behaviour.
    /// <br></br><br></br>
    /// For non tile-entities, this function simply returns - but tile entities are derived from classes that 
    /// inherit from Constructable, allowing them to extend this function.
    /// </summary>
    public override void TickInstance(Vector2Int position, Dictionary<String, object> data) {

        if ((StorageType) data[STORAGE_TYPE] == StorageType.Brood) {
            Dictionary<String, object> broodData = (Dictionary<String, object>) data[BROOD_DATA];

            int timeLeft = (int) broodData[BROOD_TIME_LEFT] - 1;
            broodData[BROOD_TIME_LEFT] = timeLeft;

            if (timeLeft <= 0) {
                EntityManager.Instance.InstantiateWorker(position);
                data[STORAGE_TYPE] = StorageType.Empty;
            }
        }
    }
}

public enum CombAttr {
    ContainsBrood,
    ContainsFor
}
