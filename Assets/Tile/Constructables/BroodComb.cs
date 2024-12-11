using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Comb")]
public class BroodComb : TileEntity, Configurable, Storage {

    // Constants for TileEntityData attribute names

    /// <summary>The nature of the contents currently stored in the comb. The corresponding value should be of type <c>StorageType</c>.</summary>
    public const String CURRENT_STORAGE_TYPE = "currentStorageType";
    public enum StorageType { Brood, Fermentable, Item, Empty };


    /// <summary>Player-set flags that indicate what types of things should be allowed to be stored in the tile entity.</summary>
    public const String CAN_STORE_BROOD = "canStoreBrood";
    public const String CAN_STORE_FERMENTABLE = "canStoreFermentable";
    public const String CAN_STORE_ITEM = "canStoreItem";


    /// <summary>Data relevant to brood stored in a comb tile. The corresponding value should be of type <c>Dictionary</c>.</summary>
    public const String BROOD_DATA = "broodData";

        /// <summary>Part of the <c>BROOD_DATA</c> attribute, storing the type of brood currently inside. Value is of type <c>BroodType</c>.</summary>
        public const String BROOD_DATA__TYPE = "broodData__broodType";
        public enum BroodType { Worker, Drone, Queen };
        /// <summary>Part of the <c>BROOD_DATA</c> attribute, storing the life stage of the brood inside. Value is of type <c>BroodStage</c>.</summary>
        public const String BROOD_DATA__BROOD_STAGE = "broodData__broodStage";
        public enum BroodStage { Egg, Larva, Pupa, };
        /// <summary>Part of <c>BROOD_DATA</c> attribute. It counts down the remaining number of ticks until the brood moves to the next life stage;
        /// see <c>BroodStage</c>. The attribute's value should be of type <c>int</c>.</summary>
        public const String BROOD_DATA__TIME_LEFT = "broodData__timeLeft";

        const int EGG_STAGE_DURATION = 80, LARVA_STAGE_DURATION = 160, PUPA_STAGE_DURATION = 160;
        const int LARVA_FEED = LARVA_STAGE_DURATION - 20, LARVA_SEAL = LARVA_STAGE_DURATION - 80;


    /// <summary>Path leading to the inventory field. The value should be of type <c>Inventory</c></summary>
    public const String INVENTORY = "inventory";


    // Specific, static attributes that may change for a type of comb, but not different instances of tile entities
    // (e.g. WorkerBroodComb vs DroneBroodComb will be different, but probably not WorkerBroodComb vs WorkerBroodComb)
    [SerializeField]
    uint maxCapacity = 100;

    [SerializeField]
    BroodCombSize broodCombSize = BroodCombSize.Worker;

    [Serializable]
    public enum BroodCombSize { Worker, Drone, Queen };

    [SerializeField]
    Item beeswax, royalJelly;

    // Visual Variants to display with a change of state

    /// <summary>Variants to display when the comb contains brood of differing life stages.</summary>
    [SerializeField]
    GridRow[] containsEggVariant, containsLarvaVariant, sealedVariant;

    public override Dictionary<String, object> GenerateDefaultData() {
        Dictionary<String, object> data = new Dictionary<String, object>();

        data[CURRENT_STORAGE_TYPE] = StorageType.Empty;

        data[CAN_STORE_BROOD] = false;
        data[CAN_STORE_FERMENTABLE] = false;
        data[CAN_STORE_ITEM] = true;

        return data;
    }

    /// <summary>
    /// Pass a tile-entity instance's data to its parent constructable, in order to execute specific behaviour.
    /// <br></br><br></br>
    /// For non tile-entities, this function simply returns - but tile entities are derived from classes that 
    /// inherit from Constructable, allowing them to extend this function.
    /// </summary>
    public override void TickInstance(Vector2Int position, Dictionary<String, object> data) {

        if ((StorageType) data[CURRENT_STORAGE_TYPE] == StorageType.Brood) TickBrood(position, data);
    }

    void TickBrood(Vector2Int position, Dictionary<String, object> data) {
        Dictionary<String, object> broodData = (Dictionary<String, object>) data[BROOD_DATA];

        int timeLeft = (int) broodData[BROOD_DATA__TIME_LEFT] - 1;
        broodData[BROOD_DATA__TIME_LEFT] = timeLeft;

        BroodStage broodStage = (BroodStage) broodData[BROOD_DATA__BROOD_STAGE];

        // Developmental events
        if (broodStage == BroodStage.Larva) {
            if (timeLeft == LARVA_FEED) {
                TaskManager.Instance.CreateTask(new NurseTask(TaskPriority.Important, position, this, royalJelly, 1));
            }

            else if (timeLeft == LARVA_SEAL) {
                TaskManager.Instance.CreateTask(new NurseTask(TaskPriority.Important, position, this, beeswax, 1));
            }
        }

        // Changing life-stages
        if (timeLeft > 0) return;

        // Egg -> Larva
        if (broodStage == BroodStage.Egg) {
            broodData[BROOD_DATA__TIME_LEFT] = LARVA_STAGE_DURATION;
            broodData[BROOD_DATA__BROOD_STAGE] = BroodStage.Larva;

            DrawVariant(position, GetTileAt__ContainsLarvaVariant);
        }

        // Larva -> Pupa
        else if (broodStage == BroodStage.Larva) {
            broodData[BROOD_DATA__TIME_LEFT] = PUPA_STAGE_DURATION;
            broodData[BROOD_DATA__BROOD_STAGE] = BroodStage.Pupa;
        }

        // Pupa -> Adult
        else {
            EntityManager.Instance.InstantiateWorker(position);
            data[CURRENT_STORAGE_TYPE] = StorageType.Empty;

            TileManager.Instance.DrawVariant(position, this, GetTileAt);
        }
    }

    TileBase GetTileAt__ContainsEggVariant(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return containsEggVariant[row].gridEntries[col].worldTile;
    }

    TileBase GetTileAt__ContainsLarvaVariant(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return containsLarvaVariant[row].gridEntries[col].worldTile;
    }

    TileBase GetTileAt__SealedVariant(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return sealedVariant[row].gridEntries[col].worldTile;
    }

    public void GiveBrood(Vector2Int position, Dictionary<String, object> data, Item item, uint quantity) {

        if (item == beeswax) {
            DrawVariant(position, GetTileAt__SealedVariant);
        }

        else Debug.Log("Fed!");
    }



    Inventory GetInventory(Dictionary<String, object> instance) {
        Inventory inventory;

        object value;
        if (instance.TryGetValue(INVENTORY, out value)) inventory = (Inventory) value;
        else {
            inventory = new Inventory(maxCapacity);
            instance[INVENTORY] = inventory;
        }

        return inventory;
    }

    public uint CountItem(Dictionary<String, object> instance, Item item) {
        return GetInventory(instance).CountItem(item);
    }

    public void Give(Vector2Int defaultLocation, Dictionary<String, object> instance, Item item, uint quantity) {
        if (!IsAvailableStorage(instance)) return;

        Inventory inventory = GetInventory(instance);
        uint space = inventory.MaxCapacity() - inventory.Carrying();

        if (space >= quantity) {
            inventory.AddAtomic(item, quantity);
        }

        else {
            inventory.AddAtomic(item, space);
            uint remaining = quantity - space;

            EntityManager.Instance.InstantiateItemEntity(defaultLocation, item, remaining);            
        }

        instance[CURRENT_STORAGE_TYPE] = StorageType.Item;
    }


    public bool Take(Dictionary<String, object> instance, Item item, uint quantity) {
        if ((StorageType) instance[CURRENT_STORAGE_TYPE] != StorageType.Item) return false;

        Inventory inventory = GetInventory(instance);
        bool success = inventory.RemoveAtomic(item, quantity);

        if (success && inventory.Carrying() == 0) instance[CURRENT_STORAGE_TYPE] = StorageType.Empty;

        return success;
    }

    public uint RemainingCapacity(Dictionary<String, object> instance) {
        Inventory inventory = GetInventory(instance);

        return inventory.MaxCapacity() - inventory.Carrying();
    }


    public bool IsAvailableStorage(Dictionary<String, object> instance) {
        if ((bool) instance[CAN_STORE_ITEM] == false) return false;

        bool success = (StorageType) instance[CURRENT_STORAGE_TYPE] == StorageType.Empty || 
               (StorageType) instance[CURRENT_STORAGE_TYPE] == StorageType.Item;

        if (!success) return false;

        if (RemainingCapacity(instance) == 0) return false;

        return true;
    }

    public bool CanStoreBrood(Dictionary<String, object> data) {
        if (data == null) return false;

        if ((StorageType) data[CURRENT_STORAGE_TYPE] != StorageType.Empty) return false;

        return (bool) data[CAN_STORE_BROOD];
    }

    public bool TryLayEgg(Vector2Int location, BroodType broodType) {

        // Get the tile entity data & check we can store brood here
        Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(location);
        if (CanStoreBrood(data) == false) return false;
        
        DrawVariant(location, GetTileAt__ContainsEggVariant);

        data[CURRENT_STORAGE_TYPE] = StorageType.Brood;

        Dictionary<String, object> broodData = new Dictionary<String, object>();
        broodData[BROOD_DATA__TIME_LEFT] = EGG_STAGE_DURATION;
        broodData[BROOD_DATA__BROOD_STAGE] = BroodStage.Egg;
        broodData[BROOD_DATA__TYPE] = broodType;
        data[BROOD_DATA] = broodData;

        return true;
    }


    public override InfoBranch GetTileEntityInfoTree(Dictionary<String, object> instance) {
        InfoBranch root = new InfoBranch(String.Empty);

        // Generic brood comb data
        InfoBranch combCategory = new InfoBranch("Comb properties");
        root.AddChild(combCategory);

        String contentsValue = instance[CURRENT_STORAGE_TYPE] switch {
            StorageType.Brood => "Brood",
            StorageType.Empty => "Nothing",
            StorageType.Fermentable => "Fermentable resources",
            StorageType.Item => "Items",
            _ => "Unknown"
        };

        InfoLeaf contentsProperty = new InfoLeaf("Currently contains", value: contentsValue);
        combCategory.AddChild(contentsProperty);

        InfoLeaf canStoreBroodProperty = new InfoLeaf("Can store brood", instance[CAN_STORE_BROOD].ToString());
        combCategory.AddChild(canStoreBroodProperty);

        InfoLeaf canStoreFermentablesProperty = new InfoLeaf("Can store fermentables", instance[CAN_STORE_FERMENTABLE].ToString());
        combCategory.AddChild(canStoreFermentablesProperty);

        InfoLeaf canStoreItemsProperty = new InfoLeaf("Can store items", instance[CAN_STORE_ITEM].ToString());
        combCategory.AddChild(canStoreItemsProperty);


        // Brood data
        if ((StorageType) instance[CURRENT_STORAGE_TYPE] == StorageType.Brood) {
            InfoBranch broodCategory = new InfoBranch("Brood properties");
            root.AddChild(broodCategory);

            Dictionary<String, object> broodData = (Dictionary<String, object>) instance[BROOD_DATA];

            String casteValue = broodData[BROOD_DATA__TYPE] switch {
                BroodType.Worker => "Worker (Honey Bee)",
                BroodType.Queen => "Queen (Honey Bee)",
                BroodType.Drone => "Drone (Honey Bee)",
                _ => "Unknown"
            };
            InfoLeaf casteProperty = new InfoLeaf("Caste", casteValue);
            broodCategory.AddChild(casteProperty);

            String stageValue = broodData[BROOD_DATA__BROOD_STAGE] switch {
                BroodStage.Egg => "Egg",
                BroodStage.Larva => "Larva",
                BroodStage.Pupa => "Pupa",
                _ => "Unknown"
            };
            InfoLeaf stageProperty = new InfoLeaf("Life stage", stageValue);
            broodCategory.AddChild(stageProperty);

            int timeLeft = (int) (TileManager.TICKS_TO_SECONDS * (int) broodData[BROOD_DATA__TIME_LEFT]);
            InfoLeaf timeProperty = new InfoLeaf("Time to next stage", timeLeft + "s");
            broodCategory.AddChild(timeProperty);
        }


        // Inventory data
        InfoBranch inventoryCategory = GetInventory(instance).GetInfoTree();
        root.AddChild(inventoryCategory);
        
        return root;
    }

    public InfoBranch GetConfigTree(Dictionary<String, object> instance) {
        InfoBranch root = new InfoBranch("Configurable properties");

        StorageType store = (StorageType) instance[CURRENT_STORAGE_TYPE];

        InfoCheckbox storeBroodProperty = new InfoCheckbox("Can store brood", 
                                                            new string[]{ CAN_STORE_BROOD }, 
                                                            (bool) instance[CAN_STORE_BROOD],
                                                            store == StorageType.Brood || store == StorageType.Empty);
        root.AddChild(storeBroodProperty);

        InfoCheckbox storeFermentablesProperty = new InfoCheckbox("Can store fermentables",
                                                            new string[]{ CAN_STORE_FERMENTABLE },
                                                            (bool) instance[CAN_STORE_FERMENTABLE],
                                                            store == StorageType.Fermentable || store == StorageType.Empty);
        root.AddChild(storeFermentablesProperty);

        InfoCheckbox storeItemsProperty = new InfoCheckbox("Can store items",
                                                            new string[]{ CAN_STORE_ITEM },
                                                            (bool) instance[CAN_STORE_ITEM],
                                                            store == StorageType.Item || store == StorageType.Empty);
        root.AddChild(storeItemsProperty);

        return root;
    }
}

public enum CombAttr {
    ContainsBrood,
    ContainsFor
}
