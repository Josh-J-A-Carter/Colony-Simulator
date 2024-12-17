using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eat__Consume : State {

    [SerializeField]
    AnimationClip anim;

    InventoryManager inventory;
    HealthComponent health;
    List<(Resource, uint)> resourceRequirements;

    const float ACTIVE_TIME = 3.0f;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();
        health = entity.GetComponent<HealthComponent>();

        resourceRequirements = new() { ( new(ItemTag.Food), 1) };
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        List<(Item, uint)> eaten = inventory.TakeResources(resourceRequirements);

    #if UNITY_EDITOR
        Debug.Assert(eaten.Count == 1);
    #endif

        (Item item, uint quantity) = eaten[0];
        health.Feed(item, quantity);
    }

    public override void FixedRun() {
        if (parent.activeFor > ACTIVE_TIME) {
            CompleteState();
        }
    }
}
