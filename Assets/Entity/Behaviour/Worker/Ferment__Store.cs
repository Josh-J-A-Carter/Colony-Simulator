using System;
using System.Collections.Generic;
using UnityEngine;

public class Ferment__Store : State {

    [SerializeField]
    AnimationClip anim;

    Vector2Int targetLocation;
    BroodComb targetType;
    Dictionary<String, object> targetData;

    Path path;
    static readonly int stepSpeed = 15;

    int pulse;

    const int PULSE_RATE = 25;

    List<(Resource, uint)> resourceRequirements;

    InventoryManager inventory;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();
        
        resourceRequirements = new() { ( new(ItemTag.Fermentable), 1) };
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        List<(Vector2Int, BroodComb, Dictionary<String, object>)> storage = TileManager.Instance.QueryTileEntities<BroodComb>(
                tuple => tuple.Item2.CanStoreFermentable(tuple.Item3)
            );

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
            path.Initialise(entity, stepSpeed);
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
            if (newData != targetData || !targetType.CanStoreFermentable(targetData)) {
                CompleteState();
                return;
            }
        }

        bool success = path.Increment();

        if (path.IsComplete()) {
            List<(Item item, uint)> taken = inventory.TakeResources(resourceRequirements);

            Vector2 pos = entity.transform.position;
            Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));
            targetType.TryStoreFermentable(gridPos, targetData, taken);

            CompleteState();
            return;
        }

        if (success == false) CompleteState();
    }
}
