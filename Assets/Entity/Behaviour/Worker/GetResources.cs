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
    int step, stepsMax;
    static readonly int stepSpeed = 15;

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
        Vector2 pos = entity.transform.position;
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));

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
                .OrderBy(entity => 
                    Math.Pow(entity.transform.position.x - gridPos.x, 2) + Math.Pow(entity.transform.position.y - gridPos.y, 2)
                )
                .ToList();

        // No entities to collect
        if (itemEntities.Count == 0) return false;
        
        // Choose the closest one that is reachable
        foreach (ItemEntity target in itemEntities) {
            path = Pathfind.FindPath(transform.position, target.transform.position);
            if (path != null) {
                targetEntity = target;
                step = 0;
                stepsMax = path.Count * stepSpeed;
                return true;
            }
        }
        
        // No valid paths to any item entities
        return false;
    }

    bool TryFindStorageTarget() {
        Vector2 pos = entity.transform.position;
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));

        // Get all possible storage locations, and filter to those that contain required resources
        List<(Vector2Int, IStorage, Dictionary<String, object>)> storage = TileManager.Instance
                .FindAvailableStorage()
                .Where(val => {
                    (_, IStorage type, Dictionary<String, object> data) = val;
                    
                    foreach ((Resource res, _) in resources) if (type.CountResource(data, res) > 0) return true;
                    
                    return false;
                })
                .OrderBy(val => {
                    (Vector2Int pos, _, _) = val;
                    return Math.Pow(pos.x - gridPos.x, 2) + Math.Pow(pos.y - gridPos.y, 2);
                })
                .ToList();

        if (storage.Count == 0) return false;

        foreach ((Vector2Int, IStorage, Dictionary<String, object>) tuple in storage) {
            (targetLocation, targetType, targetData) = tuple;

            path = Pathfind.FindPath(transform.position, targetLocation);
            if (path != null) {
                step = 0;
                stepsMax = path.Count * stepSpeed;
                return true;
            }
        }
        
        return false;
    }

    public override void FixedRun() {
        if (Pulse() == false) {
            CompleteState();
            return;
        }

        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step == stepsMax) {
            if (isTargetingItemEntity) {
                targetEntity.Collect(inventory);
            }

            else {
                // What items are left to satisfy?
                List<(Resource, uint)> remaining = inventory.FindRemainder(resources.ToList());

                // Take from storage & store temporarily, while we make sure inventory has enough space
                List<(Item, uint)> takenFromStorage = targetType.TakeResources(targetData, remaining);

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
