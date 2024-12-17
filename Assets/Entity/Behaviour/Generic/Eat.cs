using System.Collections.Generic;
using UnityEngine;

public class Eat : State {
    [SerializeField]
    GetResources getResources;

    [SerializeField]
    State consume;

    InventoryManager inventory;
    List<(Resource, uint)> resourceRequirements;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();
        resourceRequirements = new() { ( new(ItemTag.Food), 1) };
    }

    public override void OnEntry() {
        DecideState();
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (success == false) {
            CompleteState(false);
            return;
        }

        if (exitingChild == consume) {
            CompleteState();
            return;
        }

        DecideState();
    }


    void DecideState() {
        if (inventory.HasResources(resourceRequirements)) {
            stateMachine.SetChildState(consume);
        } else {
            getResources.SetResourceRequirements(resourceRequirements.AsReadOnly());
            stateMachine.SetChildState(getResources);
        }
    }

}
