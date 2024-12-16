using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Tidy__Store : State {

    [SerializeField]
    AnimationClip anim;

    Vector2Int targetLocation;
    IStorage targetType;
    Dictionary<String, object> targetData;

    Path path;

    int step, stepsMax;
    static readonly int stepSpeed = 15;

    int pulse;

    const int PULSE_RATE = 25;


    public override void OnEntry() {
        animator.Play(anim.name);

        List<(Vector2Int, IStorage, Dictionary<String, object>)> storage = TileManager.Instance.FindAvailableStorage();

        // We need somewhere to store our collected items, but there isn't anywhere - return failure
        if (storage.Count == 0) {
            CompleteState(false);
            return;
        }

        // Find a path to one of them, if possible
        (Path path, int index) = Pathfind.FindPathToOneOf(transform.position, storage, tuple => tuple.Item1, randomise: true);

        if (path != null) {
            this.path = path;
            (targetLocation, targetType, targetData) = storage[index];

            step = 0;
            stepsMax = path.Count * stepSpeed;
            return;
        }
        
        // Couldn't find a path to an item - failure to find ANY item
        CompleteState(false);
    }

    public override void FixedRun() {

        // At regular intervals, check that the tile entity still exists & is available
        pulse += 1;

        if (pulse == PULSE_RATE) {
            pulse = 0;

            // Tile entity doesn't exist anymore or is no longer available, leave the state
            // Not necessarily failure to find ANY storage, so don't return failure
            Dictionary<String, object> newData = TileManager.Instance.GetTileEntityData(targetLocation);
            if (newData != targetData || !targetType.IsAvailableStorage(targetData)) {
                CompleteState();
                return;
            }
        }

        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step > stepsMax) {
            // Get the first N items in inventory (N = space available in storage)
            int space = (int) targetType.RemainingCapacity(targetData);

            List<(Item, uint)> toStore = new();

            foreach ((Item item, uint quantity) in inventory.GetContents()) {
                if (quantity < space) {
                    toStore.Add((item, quantity));
                    space -= (int) quantity;
                    continue;
                }

                toStore.Add((item, (uint) space));
                break;
            }

            foreach ((Item item, uint quantity) in toStore) {
                targetType.Give(targetLocation, targetData, item, quantity);
                inventory.Take(item, quantity);
            }

            CompleteState();
            return;
        }

        if (success == false) CompleteState();
    }
}
