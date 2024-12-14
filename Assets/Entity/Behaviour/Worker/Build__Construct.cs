using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Build__Construct : State {
    BuildTask task => (BuildTask) taskAgent.GetTask();

    [SerializeField]
    AnimationClip anim;

    public override void OnEntry() {
        animator.Play(anim.name);

        // Make sure we can actually complete the task lol
        if (!task.HasAllocation()) {
            foreach ((Item item, uint quantity) in task.GetRequiredResources()) {
                if (!inventory.Has(item, quantity)) {
                    CompleteState(false);
                    return;
                }
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
