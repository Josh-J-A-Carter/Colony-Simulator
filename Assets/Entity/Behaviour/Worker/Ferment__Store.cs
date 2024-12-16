using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ferment__Store : State {

    [SerializeField]
    AnimationClip anim;

    Vector2Int targetLocation;
    BroodComb targetType;
    Dictionary<String, object> targetData;

    Path path;
    const int TARGET_ATTEMPTS = 10;

    int step, stepsMax;
    static readonly int stepSpeed = 15;

    int pulse;

    const int PULSE_RATE = 25;

    List<(Resource, uint)> resourceRequirements;

    public void Awake() {
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

        // Choose some (limited) random item entities
        List<(Vector2Int, BroodComb, Dictionary<String, object>)> randomTargets = new(TARGET_ATTEMPTS);

        for (int i = 0 ; i < TARGET_ATTEMPTS && i < storage.Count ; i += 1) {
            randomTargets.Add(storage[Random.Range(0, storage.Count)]);
        }

        // Order them by proximity to the agent; we shouldn't be going for items 20000 tiles away when there is one next to us
        Vector2 pos = entity.transform.position;
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));
        List<(Vector2Int, BroodComb, Dictionary<String, object>)> orderedTargets = randomTargets
                .OrderBy(tuple => Vector2.Distance(tuple.Item1, gridPos))
                .ToList();
        
        // Choose the closest one that is reachable
        foreach ((Vector2Int loc, BroodComb type, Dictionary<String, object> data) in orderedTargets) {
            targetLocation = loc;
            targetType = type;
            targetData = data;

            path = Pathfind.FindPath(transform.position, targetLocation);
            if (path != null) {
                step = 0;
                stepsMax = path.Count * stepSpeed;
                return;
            }
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

        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step == stepsMax) {
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
