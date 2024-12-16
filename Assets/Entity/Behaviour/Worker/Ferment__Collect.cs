using System;
using System.Collections.Generic;
using UnityEngine;

public class Ferment__Collect : State {

    [SerializeField]
    AnimationClip anim;

    Vector2Int targetLocation;
    BroodComb targetType;
    Dictionary<String, object> targetData;

    Path path;

    int step, stepsMax;
    static readonly int stepSpeed = 15;

    int pulse;

    const int PULSE_RATE = 25;

    public override void OnEntry() {
        animator.Play(anim.name);

        List<(Vector2Int, BroodComb, Dictionary<String, object>)> storage = TileManager.Instance.QueryTileEntities<BroodComb>(
                tuple => tuple.Item2.FermentablesReady(tuple.Item3)
            );

        // We need somewhere to store our collected items, but there isn't anywhere - return failure
        if (storage.Count == 0) {
            CompleteState(false);
            return;
        }

        // Find a path to one of them, if possible
        (Path path, int index) = Pathfind.FindPathToOneOf(transform.position, storage, tuple => tuple.Item1, true);

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
            if (newData != targetData || !targetType.FermentablesReady(targetData)) {
                CompleteState();
                return;
            }
        }

        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step == stepsMax) {
            Vector2 pos = entity.transform.position;
            Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));
            List<(Item item, uint)> fermentables = targetType.CollectFermentables(gridPos, targetData);
            
            inventory.Give(fermentables);

            CompleteState();
            return;
        }

        if (success == false) CompleteState();
    }
}
