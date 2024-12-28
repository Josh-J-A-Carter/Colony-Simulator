using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Nurse : State {

    [SerializeField]
    State pathfind, administer;

    [SerializeField]
    GetResources getResources;

    NurseTask task => (NurseTask) taskAgent.GetTask();

    ITaskAgent taskAgent;
    InventoryManager inventory;

    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
        inventory = entity.GetComponent<InventoryManager>();

        (pathfind as Locative__Pathfind).TargetInteriorPoints();
    }
 
    public override void OnEntry() {
        if (inventory.HasResources(task.GetRequiredResources().ToList())) stateMachine.SetChildState(pathfind);
        else {
            getResources.SetResourceRequirements(task.GetRequiredResources());
            stateMachine.SetChildState(getResources);
        }
    }

    public override void OnChildExit(State exitingChild, bool success = true) {
        if (!success) {
            taskAgent.CancelAssignment();
            CompleteState(false);
            return;
        }

        if (exitingChild == getResources) {
            if (inventory.HasResources(task.GetRequiredResources().ToList())) stateMachine.SetChildState(pathfind);
            else {
                getResources.SetResourceRequirements(task.GetRequiredResources());
                stateMachine.SetChildState(getResources);
            }
        }
    
        else if (exitingChild == pathfind) stateMachine.SetChildState(administer);
        else if (exitingChild == administer) CompleteState();
    }
}
