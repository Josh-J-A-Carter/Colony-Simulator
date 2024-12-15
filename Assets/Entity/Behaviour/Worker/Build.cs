using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Build : State {

    [SerializeField]
    State getResources, pathfind, construct;
    BuildTask task => (BuildTask) taskAgent.GetTask();

    const float MAX_IDLE = 2.0f;
    
    public override void OnEntry() {
        if (HaveRequirements()) stateMachine.SetChildState(pathfind);
        else stateMachine.SetChildState(getResources);
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (!success) {
            taskAgent.CancelAssignment();
            CompleteState(false);
            return;
        }

        if (exitingChild == getResources) {
            if (HaveRequirements()) stateMachine.SetChildState(pathfind);
            else stateMachine.SetChildState(getResources);
        }
    
        else if (exitingChild == pathfind) stateMachine.SetChildState(construct);
        else if (exitingChild == construct) CompleteState();
    }

    bool HaveRequirements() {
        foreach ((Item item, uint quantity) in task.GetRequiredResources()) {
            if (inventory.Has(item, quantity) == false) return false;
        }

        return true;
    }
}
