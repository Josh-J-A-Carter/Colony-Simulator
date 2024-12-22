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
    static readonly int stepSpeed = 15;

    InventoryManager inventory;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();
    }

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

        bool success = path.Increment();

        if (path.IsComplete()) {
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

        // Find a path to one of them, if possible
        (Path path, int index) = Pathfind.FindPathToOneOf(transform.position, itemEntities.ToList(), entity => entity.transform.position, randomise: true);

        if (path != null) {
            this.path = path;
            targetEntity = itemEntities[index];
            path.Initialise(entity, stepSpeed);
            return;
        }
        
        // Couldn't find a path to an item - failure to find ANY item
        CompleteState(false);
    }
}
