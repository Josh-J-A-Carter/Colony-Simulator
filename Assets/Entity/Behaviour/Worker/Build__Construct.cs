using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            if (inventory.HasResources(task.GetRequiredResources().ToList()) == false) {
                CompleteState(false);
                return;
            }

            TaskManager.Instance.Allocate(task, inventory);
        }
    }

    public override void FixedRun(){
        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
