using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ferment : State {

    [SerializeField]
    State store;

    [SerializeField]
    GetResources getResources;
    List<(Resource, uint)> resourceRequirements;

    public void Awake() {
        resourceRequirements = new() { ( new(ItemTag.Fermentable), 1) };
    }
    
    public override void OnEntry() {
        if (inventory.HasResources(resourceRequirements)) stateMachine.SetChildState(store);
        else {
            getResources.SetResourceRequirements(resourceRequirements.AsReadOnly());
            stateMachine.SetChildState(getResources);
        }
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (!success) {
            CompleteState(false);
            return;
        }

        if (exitingChild == getResources) {
            if (inventory.HasResources(resourceRequirements)) stateMachine.SetChildState(store);
            else {
                getResources.SetResourceRequirements(resourceRequirements.AsReadOnly());
                stateMachine.SetChildState(getResources);
            }
        }
    
        else if (exitingChild == store) CompleteState();
    }
}
