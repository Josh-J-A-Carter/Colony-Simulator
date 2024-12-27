using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

    public static ResourceManager Instance { get; private set; }

    // Dictionary<Item, uint> itemRecord;

    // List<ItemEntity> itemEntities;
    // List<Vector2Int> storageTileEntities;

    public void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        // itemRecord = new();
        // itemEntities = new();
        // storageTileEntities = new();
    }

    public bool Available(InventoryManager inventory, IEnumerable<(Resource, uint)> resources) {
        foreach ((Resource res, uint quantity) in resources) if (Available(inventory, res, quantity) == false) return false;

        return true;
    }

    public bool Available(InventoryManager inventory, Resource resource, uint quantity = 1) {
        ///
        /// Note: potential source of bug here - we aren't adding up the subtotals across
        /// inventory, entities, and storage; e.g. could have 1/3 in each but the test would fail
        ///

        if (inventory.CountResource(resource) >= quantity) return true;

        if (EntityManager.Instance.FindItemEntities(resource, quantity, out _)) return true;

        if (TileManager.Instance.FindResourceInStorage(resource, quantity, out _)) return true;

        return false;
    }

//     /// <summary>
//     /// In the nest, are there at least <c>quantity</c> units of <c>item</c>, as item entities or in storage?
//     /// </summary>
//     // public bool ExistenceQuery(Item item, uint quantity) {
//     //     uint existingQuantity = itemRecord.GetValueOrDefault(item, (uint) 0);

//     //     return quantity >= existingQuantity;
//     // }

//     public bool FindItemEntities(Item item, uint quantity, out List<ItemEntity> result) {
//         result = new();
//         int target = (int) quantity;
        
//         foreach (ItemEntity entity in itemEntities) {
//             if (entity.item != item) continue;

//             result.Add(entity);
//             target -= (int) entity.quantity;

//             // Return early if we already reach the target
//             if (target <= 0) return true;
//         }

//         return false;
//     }

//     public bool FindStorage(Item item, uint quantity, out List<Vector2Int> result) {
//         result = new();
//         int target = (int) quantity;
        
//         foreach (Vector2Int pos in storageTileEntities) {
//             (_, Constructable constructable) = TileManager.Instance.GetConstructableAt(pos);
//             Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(pos);

//             if (constructable is Storage storage) {
//                 Inventory inventory = storage.GetInventory(data);
//                 uint contribution = inventory.CountItem(item);
                
//                 if (contribution == 0) continue;

//                 result.Add(pos);
//                 target -= (int) contribution;
//             }

//             // Return early if we already reach the target
//             if (target <= 0) return true;
//         }

//         return false;
//     }

//     // public void UpdateCount(Item item, int deltaQuantity) {
//     //     uint oldQuantity = itemRecord.GetValueOrDefault(item, (uint) 0);

//     //     itemRecord[item] = (uint) (oldQuantity + deltaQuantity);
//     // }

//     public void Register(ItemEntity itemEntity) {
//         itemEntities.Add(itemEntity);
//         // UpdateCount(itemEntity.item, (int) itemEntity.quantity);
//     }

//     public void Register(Vector2Int storageTileEntity) {
//         storageTileEntities.Add(storageTileEntity);

//         // We actually expect the tile entity to be storing nothing on initialisation, but you never know
//         // foreach ((Item item, uint quantity) in inventoryContents) UpdateCount(item, (int) quantity);
//     }

//     public void Deregister(ItemEntity itemEntity) {
//         bool success = itemEntities.Remove(itemEntity);

//         if (!success) return;

//         // UpdateCount(itemEntity.item, (int) -itemEntity.quantity);
//     }

//     public void Deregister(Vector2Int storageTileEntity) {
//         bool success = storageTileEntities.Remove(storageTileEntity);

//         if (!success) return;

//         // foreach ((Item item, uint quantity) in inventoryContents) UpdateCount(item, (int) -quantity);
//     }
}
