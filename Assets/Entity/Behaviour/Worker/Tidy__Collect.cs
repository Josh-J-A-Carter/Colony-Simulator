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
    int TARGET_ATTEMPTS = 10;

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

        int currentX = (int) Math.Floor(entity.transform.position.x);
        int currentY = (int) Math.Floor(entity.transform.position.y);

        Vector2Int current = new Vector2Int(currentX, currentY);

        if (path.IsValidFrom(current)) {
            
            // Linearly interpolate to next point
            Vector2 nextPoint = path.LinearlyInterpolate(step, stepsMax);
            Vector2 translation = nextPoint - (Vector2) entity.transform.position;
            entity.transform.Translate(translation);

            /// Remember to flip the character's sprite as needed
            int sign = Math.Sign(translation.x);
            if (sign != 0) entity.transform.localScale = new Vector3(sign, 1, 1);

            step += 1;

            // Once we reach the end of the path, leave state & collect item
            if (step > stepsMax) {
                targetEntity.Collect(inventory);
                CompleteState();
            }

        } 
        
        // Path is invalidated - but there may exist another valid path so don't return failure here
        else CompleteState();
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
