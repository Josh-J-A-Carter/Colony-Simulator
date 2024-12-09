using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Random = UnityEngine.Random;


public class Tidy__Collect : State {
    
    [SerializeField]
    AnimationClip anim;

    ReadOnlyCollection<ItemEntity> itemEntities;
    ItemEntity targetEntity;
    Path path;
    int TARGET_ATTEMPTS = 5;

    int step, stepsMax;
    static readonly int stepSpeed = 15;



    public override void OnEntry() {
        animator.Play(anim.name);

        itemEntities = EntityManager.Instance.GetItemEntities();

        if (itemEntities.Count == 0) {
            CompleteState();
            return;
        }

        for (int i = 0 ; i < TARGET_ATTEMPTS ; i += 1) {
            targetEntity = itemEntities[Random.Range(0, itemEntities.Count)];

            path = Pathfind.FindPath(transform.position, targetEntity.transform.position);
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
}
