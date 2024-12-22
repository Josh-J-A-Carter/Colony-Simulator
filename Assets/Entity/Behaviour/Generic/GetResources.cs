using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;


public class GetResources : State {
    
    [SerializeField]
    AnimationClip anim;

    ReadOnlyCollection<(Resource, uint)> resources;

    bool isTargetingItemEntity;
    ItemEntity targetEntity;

    IStorage targetType;
    Dictionary<String, object> targetData;
    Vector2Int targetLocation;
    int pulse = 0;
    const int PULSE_RATE = 25;
    
    
    Path path;
    static readonly int stepSpeed = 15;

    InventoryManager inventory;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();
    }

    public void SetResourceRequirements(ReadOnlyCollection<(Resource, uint)> resources) {
        this.resources = resources;
    }


    public override void OnEntry() {
    #if UNITY_EDITOR
        Debug.Assert(resources != null);
    #endif

        animator.Play(anim.name);

        bool success = TryFindItemTarget();
        isTargetingItemEntity = success;
        if (success) return;

        success = TryFindStorageTarget();
        if (success) return;

        CompleteState(false);
    }

    public override void OnExit() {
        resources = null;
    }

    bool TryFindItemTarget() {
        // Find all item entities that are relevant to the task requirements
        List<ItemEntity> itemEntities = EntityManager.Instance
                .GetItemEntities()
                .Where(entity => {
                    foreach ((Resource res, _) in resources) {
                        if (res.ResourceType == ResourceType.Item && entity.item == res.Item ||
                        res.ResourceType == ResourceType.Tag && entity.item.HasItemTag(res.ItemTag)) return true;
                    }
                    return false;
                })
                .ToList();

        // No entities to collect
        if (itemEntities.Count == 0) return false;
        
        // Find a path to one of them, if possible
        (Path path, int index) = Pathfind.FindPathToOneOf(transform.position, itemEntities.ToList(), entity => entity.transform.position, randomise: true);

        if (path != null) {
            this.path = path;
            targetEntity = itemEntities[index];
            path.Initialise(entity, stepSpeed);
            return true;
        }
        
        // No valid paths to any item entities
        return false;
    }

    bool TryFindStorageTarget() {
        // Get all possible storage locations, and filter to those that contain required resources
        List<(Vector2Int, IStorage, Dictionary<String, object>)> storage = TileManager.Instance
                .FindAvailableStorage()
                .Where(val => {
                    (_, IStorage type, Dictionary<String, object> data) = val;
                    foreach ((Resource res, _) in resources) if (type.CountResource(data, res) > 0) return true;
                    return false;
                })
                .ToList();

        if (storage.Count == 0) return false;

        (Path path, int index) = Pathfind.FindPathToOneOf(transform.position, storage, tuple => tuple.Item1);

        if (path != null) {
            this.path = path;
            (targetLocation, targetType, targetData) = storage[index];
            path.Initialise(entity, stepSpeed);
            return true;
        }

        return false;
    }

    public override void FixedRun() {
        if (Pulse() == false) {
            CompleteState();
            return;
        }

        bool success = path.Increment();

        if (path.IsComplete()) {
            if (isTargetingItemEntity) {
                targetEntity.Collect(inventory);
            }

            else {
                // What items are left to satisfy?
                List<(Resource, uint)> remaining = inventory.FindRemainder(resources.ToList());

                // Take from storage & store temporarily, while we make sure inventory has enough space
                List<(Item, uint)> takenFromStorage = targetType.TakeResources(targetLocation, targetData, remaining);

                // Swap extraneous stuff, if necessary
                // We can guarantee that storage has spaceRequired spaces free, so these can be used to swap
                uint spaceRequired = takenFromStorage.Aggregate<(Item, uint), uint>(0, (acc, tuple) => acc + tuple.Item2);
                uint capacity = inventory.RemainingCapacity();
                if (spaceRequired > capacity) {
                    // To make this easier, we temporarily remove the existing required items (TempR) from inventory,
                    // remove some random junk (RJ), put the random junk (RJ) in storage, and re-add the temporarily removed items (TempR)
                    List<(Item, uint)> tempRemoval = inventory.TakeResources(resources.ToList());

                    List<(Item, uint)> randomRemoval = inventory.RemoveN(spaceRequired - capacity);

                    targetType.Give(targetLocation, targetData, randomRemoval);

                    inventory.Give(tempRemoval);
                }

                // Take the temporarily stored items and put them in inventory
                inventory.Give(takenFromStorage);
            }

            CompleteState();
        }

        if (success == false) CompleteState();
    }

    /// <summary>
    /// Does the target still exist / is it unchanged?
    /// </summary>
    bool Pulse() {
        if (isTargetingItemEntity) return targetEntity;

        // Otherwise, we are checking storage
        pulse += 1;

        if (pulse == PULSE_RATE) {
            pulse = 0;

            // Does tile entity still exist?
            Dictionary<String, object> newData = TileManager.Instance.GetTileEntityData(targetLocation);
            if (newData != targetData || !targetType.IsAvailableStorage(targetData)) return false;
        }

        return true;
    }
}
