using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Build : State {

    [SerializeField]
    State pathfind, construct;

    [SerializeField]
    GetResources getResources;
    BuildTask task => (BuildTask) taskAgent.GetTask();

    public ITaskAgent taskAgent;
    public InventoryManager inventory;

    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
        inventory = entity.GetComponent<InventoryManager>();
    }
    
    public override void OnEntry() {
        if (inventory.HasResources(task.GetRequiredResources().ToList())) stateMachine.SetChildState(pathfind);
        else {
            getResources.SetResourceRequirements(task.GetRequiredResources());
            stateMachine.SetChildState(getResources);
        }
    }

    public override void OnChildExit(State exitingChild, bool success) {
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
    
        else if (exitingChild == pathfind) stateMachine.SetChildState(construct);
        else if (exitingChild == construct) CompleteState();
    }
}
