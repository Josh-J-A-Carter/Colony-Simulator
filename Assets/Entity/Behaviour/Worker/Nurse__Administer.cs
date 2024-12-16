using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Nurse__Administer : State {
    NurseTask task => (NurseTask) taskAgent.GetTask();

    [SerializeField]
    AnimationClip anim;

    ITaskAgent taskAgent;
    InventoryManager inventory;

    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
        inventory = entity.GetComponent<InventoryManager>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        // Make sure we can actually complete the task lol
        if (!task.HasAllocation()) {
            if (inventory.HasResources(task.GetRequiredResources().ToList()) == false) {
                CompleteState(false);
                return;
            }

            TaskManager.Instance.Allocate(task, inventory);
        }

    }

    public override void FixedRun(){
        if (task == null) return;

        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
