using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Brood Comb Constructable")]
public class BroodComb : TileEntity, IConfigurable, IStorage {

    // Constants for TileEntityData attribute names

    /// <summary>The nature of the contents currently stored in the comb. The corresponding value should be of type <c>StorageType</c>.</summary>
    public const String CURRENT_STORAGE_TYPE = "currentStorageType";
    public enum StorageType { Brood, Fermentable, Item, Empty };


    /// <summary>Player-set flags that indicate what types of things should be allowed to be stored in the tile entity.</summary>
    public const String CAN_STORE_BROOD = "canStoreBrood";
    public const String CAN_STORE_FERMENTABLE = "canStoreFermentable";
    public const String CAN_STORE_ITEM = "canStoreItem";


    /// Data relevant to brood stored in a comb tile.

        /// <summary>Part of the <c>BROOD_DATA</c> attribute, storing the type of brood currently inside. Value is of type <c>bool</c>.</summary>
        public const String BROOD_DATA__FERTILISED = "broodData__fertilised";
        /// <summary>Part of the <c>BROOD_DATA</c> attribute, storing the life stage of the brood inside. Value is of type <c>BroodStage</c>.</summary>
        public const String BROOD_DATA__BROOD_STAGE = "broodData__broodStage";
        public enum BroodStage { Egg, Larva, Pupa, };
        /// <summary>Part of <c>BROOD_DATA</c> attribute. It counts down the remaining number of ticks until the brood moves to the next life stage;
        /// see <c>BroodStage</c>. The attribute's value should be of type <c>int</c>.</summary>
        public const String BROOD_DATA__TIME_LEFT = "broodData__timeLeft";

        const int EGG_STAGE_DURATION = 80, LARVA_STAGE_DURATION = 160, PUPA_STAGE_DURATION = 160;
        const int LARVA_FEED = LARVA_STAGE_DURATION - 20, LARVA_SEAL = LARVA_STAGE_DURATION - 80;


    /// Data for stored fermentables
    
        /// <summary>Value is a <c>List</c> of tuples <c>(Item, uint)</c></summary>
        public const String FERMENTABLE_DATA__ITEMS = "fermentableData__items";

        /// <summary>Value is of type int</summary>
        public const String FERMENTABLE_DATA__TIME_LEFT = "fermentableData__timeLeft";

        const int FERMENTATION_TIME = 100;


    /// <summary>Path leading to the inventory field. The value should be of type <c>Inventory</c></summary>
    public const String INVENTORY = "inventory";


    // Specific, static attributes that may change for a type of comb, but not different instances of tile entities
    // (e.g. WorkerBroodComb vs DroneBroodComb will be different, but probably not WorkerBroodComb vs WorkerBroodComb)
    [SerializeField]
    uint maxCapacity = 100;

    [SerializeField]
    BroodCombSize broodCombSize = BroodCombSize.Worker;

    [SerializeField]
    public bool toFertilise = true;

    [Serializable]
    public enum BroodCombSize { Worker, Drone, Queen };

    [SerializeField]
    Item beeswax, royalJelly;

    // Visual Variants to display with a change of state

    /// <summary>Variants to display when the comb contains brood of differing life stages.</summary>
    [SerializeField]
    GridRow[] eggVariant, larvaVariant, larvaFedVariant, cappedVariant;

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

        if ((StorageType) data[CURRENT_STORAGE_TYPE] == StorageType.Fermentable) TickFermentable(data);
    }

    void TickBrood(Vector2Int position, Dictionary<String, object> data) {
        int timeLeft = (int) data[BROOD_DATA__TIME_LEFT] - 1;
        data[BROOD_DATA__TIME_LEFT] = timeLeft;

        BroodStage broodStage = (BroodStage) data[BROOD_DATA__BROOD_STAGE];

        // Developmental events
        if (broodStage == BroodStage.Larva) {
            if (timeLeft == LARVA_FEED) {
                List<(Resource, uint)> res = new() { (new(royalJelly), 1) };

                TaskManager.Instance.CreateTask(new NurseTask(TaskPriority.Important, position, this, res));
            }

            else if (timeLeft == LARVA_SEAL) {
                List<(Resource, uint)> res = new() { (new(beeswax), 1) };

                TaskManager.Instance.CreateTask(new NurseTask(TaskPriority.Important, position, this, res));
            }
        }

        // Changing life-stages
        if (timeLeft > 0) return;

        // Egg -> Larva
        if (broodStage == BroodStage.Egg) {
            data[BROOD_DATA__TIME_LEFT] = LARVA_STAGE_DURATION;
            data[BROOD_DATA__BROOD_STAGE] = BroodStage.Larva;

            DrawVariant(position, GetTileAt__LarvaVariant);
        }

        // Larva -> Pupa
        else if (broodStage == BroodStage.Larva) {
            data[BROOD_DATA__TIME_LEFT] = PUPA_STAGE_DURATION;
            data[BROOD_DATA__BROOD_STAGE] = BroodStage.Pupa;
        }

        // Pupa -> Adult
        else {
            bool fertilised = (bool) data[BROOD_DATA__FERTILISED];
            if (fertilised) EntityManager.Instance.InstantiateWorker(position);
            else EntityManager.Instance.InstantiateDrone(position);

            data[CURRENT_STORAGE_TYPE] = StorageType.Empty;

            TileManager.Instance.DrawVariant(position, this, GetTileAt);
        }
    }

    void TickFermentable(Dictionary<String, object> instance) {
        int timeLeft = (int) instance[FERMENTABLE_DATA__TIME_LEFT];

        timeLeft = timeLeft <= 0 ? 0 : timeLeft - 1;
        instance[FERMENTABLE_DATA__TIME_LEFT] = timeLeft;

        if (timeLeft <= 0) {
            Debug.Log("Ready for collection :)");
        }
    }

    TileBase GetTileAt__EggVariant(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return eggVariant[row].gridEntries[col].worldTile;
    }

    TileBase GetTileAt__LarvaVariant(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return larvaVariant[row].gridEntries[col].worldTile;
    }

    TileBase GetTileAt__LarvaFedVariant(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return larvaFedVariant[row].gridEntries[col].worldTile;
    }

    TileBase GetTileAt__CappedVariant(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return cappedVariant[row].gridEntries[col].worldTile;
    }

    public void GiveBrood(Vector2Int position, Dictionary<String, object> data, Item item, uint quantity) {

        if (item == beeswax) {
            DrawVariant(position, GetTileAt__CappedVariant);
        }

        else if (item == royalJelly) {
            DrawVariant(position, GetTileAt__LarvaFedVariant);
        }
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

    public uint CountResource(Dictionary<String, object> instance, Resource res) {
        return GetInventory(instance).CountResource(res);
    }

    public bool HasResources(Dictionary<String, object> instance, List<(Resource, uint)> resources) {
        return GetInventory(instance).HasResources(resources);
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

    public void Give(Vector2Int defaultLocation, Dictionary<String, object> instance, List<(Item, uint)> items) {
        foreach ((Item item, uint quantity) in items) Give(defaultLocation, instance, item, quantity);
    }


    public bool Take(Dictionary<String, object> instance, Item item, uint quantity) {
        if ((StorageType) instance[CURRENT_STORAGE_TYPE] != StorageType.Item) return false;

        Inventory inventory = GetInventory(instance);
        bool success = inventory.RemoveAtomic(item, quantity);

        if (success && inventory.Carrying() == 0) instance[CURRENT_STORAGE_TYPE] = StorageType.Empty;

        return success;
    }

    public List<(Item, uint)> TakeResources(Dictionary<String, object> instance, List<(Resource, uint)> resources) {
        Inventory inventory = GetInventory(instance);
        List<(Item, uint)> taken = inventory.TakeResources(resources);

        if (inventory.Carrying() == 0) instance[CURRENT_STORAGE_TYPE] = StorageType.Empty;

        return taken;
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

    public bool CanStoreFermentable(Dictionary<String, object> data) {
        if (data == null) return false;

        if ((StorageType) data[CURRENT_STORAGE_TYPE] != StorageType.Empty) return false;

        return (bool) data[CAN_STORE_FERMENTABLE];
    }

    public bool CanStoreBrood(Dictionary<String, object> data) {
        if (data == null) return false;

        if ((StorageType) data[CURRENT_STORAGE_TYPE] != StorageType.Empty) return false;

        return (bool) data[CAN_STORE_BROOD];
    }

    public bool TryStoreFermentable(Dictionary<String, object> instance, List<(Item, uint)> fermentables) {
        if (CanStoreFermentable(instance) == false) return false;

        instance[CURRENT_STORAGE_TYPE] = StorageType.Fermentable;

        instance[FERMENTABLE_DATA__ITEMS] = fermentables;
        instance[FERMENTABLE_DATA__TIME_LEFT] = FERMENTATION_TIME;

        return true;
    }

    public bool TryLayEgg(Vector2Int location, bool fertilised) {

        // Get the tile entity data & check we can store brood here
        Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(location);
        if (CanStoreBrood(data) == false) return false;
        
        DrawVariant(location, GetTileAt__EggVariant);

        data[CURRENT_STORAGE_TYPE] = StorageType.Brood;

        data[BROOD_DATA__TIME_LEFT] = EGG_STAGE_DURATION;
        data[BROOD_DATA__BROOD_STAGE] = BroodStage.Egg;
        data[BROOD_DATA__FERTILISED] = fertilised;

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

            String fertilisedValue = instance[BROOD_DATA__FERTILISED].ToString();
            InfoLeaf fertilisedProperty = new InfoLeaf("Fertilised", fertilisedValue);
            broodCategory.AddChild(fertilisedProperty);

            String stageValue = instance[BROOD_DATA__BROOD_STAGE] switch {
                BroodStage.Egg => "Egg",
                BroodStage.Larva => "Larva",
                BroodStage.Pupa => "Pupa",
                _ => "Unknown"
            };
            InfoLeaf stageProperty = new InfoLeaf("Life stage", stageValue);
            broodCategory.AddChild(stageProperty);

            int timeLeft = (int) (TileManager.TICKS_TO_SECONDS * (int) instance[BROOD_DATA__TIME_LEFT]);
            InfoLeaf timeProperty = new InfoLeaf("Time to next stage", timeLeft + "s");
            broodCategory.AddChild(timeProperty);
        }

        // Fermentable data
        if ((StorageType) instance[CURRENT_STORAGE_TYPE] == StorageType.Fermentable) {
            InfoBranch fermentableCategory = new InfoBranch("Fermentable properties");
            root.AddChild(fermentableCategory);

            int timeLeft = (int) (TileManager.TICKS_TO_SECONDS * (int) instance[FERMENTABLE_DATA__TIME_LEFT]);

            if (timeLeft <= 0) {
                InfoLeaf timeProperty = new InfoLeaf("Ready for collection");
                fermentableCategory.AddChild(timeProperty);
            } else {
                InfoLeaf timeProperty = new InfoLeaf("Time until completion", timeLeft + "s");
                fermentableCategory.AddChild(timeProperty);
            }

            InfoBranch storingCategory = new InfoBranch("Fermenting items");
            fermentableCategory.AddChild(storingCategory);

            foreach ((Item item, uint quantity) in (List<(Item, uint)>) instance[FERMENTABLE_DATA__ITEMS]) {
                InfoLeaf itemProperty = new InfoLeaf(item.GetName(), quantity + " unit(s)");
                storingCategory.AddChild(itemProperty);
            }
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
