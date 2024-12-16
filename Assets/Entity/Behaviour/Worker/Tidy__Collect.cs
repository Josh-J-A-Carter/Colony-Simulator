using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class Tidy__Collect : State {
    
    [SerializeField]
    AnimationClip anim;

    ReadOnlyCollection<ItemEntity> itemEntities;
    ItemEntity targetEntity;
    Path path;
    const int TARGET_ATTEMPTS = 10;

    int step, stepsMax;
    static readonly int stepSpeed = 15;


    public override void OnEntry() {
        animator.Play(anim.name);

        TryFindPath();
    }

    public override void FixedRun() {
        // If the entity doesn't exist anymore, leave the state
        // Not necessarily failure to find ANY item, so don't return failure
        if (!targetEntity) {
            CompleteState();
            return;
        }

        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step > stepsMax) {
            CompleteState();
            targetEntity.Collect(inventory);
            return;
        }

        if (success == false) TryFindPath();
    }

    void TryFindPath() {
        itemEntities = EntityManager.Instance.GetItemEntities();

        if (itemEntities.Count == 0) {
            CompleteState();
            return;
        }

        // Choose some (limited) random item entities
        List<ItemEntity> randomTargets = new List<ItemEntity>(TARGET_ATTEMPTS);

        for (int i = 0 ; i < TARGET_ATTEMPTS && i < itemEntities.Count ; i += 1) {
            ItemEntity target = itemEntities[Random.Range(0, itemEntities.Count)];

            randomTargets.Add(target);
        }

        // Order them by proximity to the agent; we shouldn't be going for items 20000 tiles away when there is one next to us
        Vector2 pos = entity.transform.position;
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));
        List<ItemEntity> orderedTargets = randomTargets
                .OrderBy(entity => Math.Pow(entity.transform.position.x - gridPos.x, 2) + Math.Pow(entity.transform.position.y - gridPos.y, 2))
                .ToList();
        
        // Choose the closest one that is reachable
        foreach (ItemEntity target in orderedTargets) {
            path = Pathfind.FindPath(transform.position, target.transform.position);
            if (path != null) {
                targetEntity = target;
                step = 0;
                stepsMax = path.Count * stepSpeed;
                return;
            }
        }
        
        // Couldn't find a path to an item - failure to find ANY item
        CompleteState(false);
    }
}
