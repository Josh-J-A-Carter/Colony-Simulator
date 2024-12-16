using System.Collections.Generic;
using UnityEngine;

public class Eat : State {
    [SerializeField]
    GetResources getResources;

    InventoryManager inventory;
    HealthComponent health;

    List<(Resource, uint)> resourceRequirements;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();
        health = entity.GetComponent<HealthComponent>();

        resourceRequirements = new() { ( new(ItemTag.Food), 1) };
    }

    public override void OnEntry() {
        DecideState();
    }

    public override void OnChildExit(State _, bool success) {
        if (success == false) {
            CompleteState(false);
            return;
        }

        DecideState();
    }


    void DecideState() {
        if (inventory.HasResources(resourceRequirements)) {
            List<(Item, uint)> eaten = inventory.TakeResources(resourceRequirements);

        #if UNITY_EDITOR
            Debug.Assert(eaten.Count == 1);
        #endif

            (Item item, uint quantity) = eaten[0];
            health.Feed(item, quantity);

            CompleteState();
        } else {
            getResources.SetResourceRequirements(resourceRequirements.AsReadOnly());
            stateMachine.SetChildState(getResources);
        }
    }

}
