using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nurse : State {

    [SerializeField]
    State getResources, pathfind, administer;

    NurseTask task => (NurseTask) taskAgent.GetTask();
 
    public override void OnEntry() {
        if (HaveRequirements()) stateMachine.SetChildState(pathfind);
        else stateMachine.SetChildState(getResources);
    }

    public override void OnChildExit(State exitingChild, bool success = true) {
        if (!success) {
            taskAgent.CancelAssignment();
            CompleteState(false);
            return;
        }

        if (exitingChild == getResources) {
            if (HaveRequirements()) stateMachine.SetChildState(pathfind);
            else stateMachine.SetChildState(getResources);
        }
    
        else if (exitingChild == pathfind) stateMachine.SetChildState(administer);
        else if (exitingChild == administer) CompleteState();
    }

    bool HaveRequirements() {
        foreach ((Item item, uint quantity) in task.GetRequiredResources()) {
            if (inventory.Has(item, quantity) == false) return false;
        }

        return true;
    }
}
