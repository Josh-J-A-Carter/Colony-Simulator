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

        TaskManager.Instance.Allocate(task, inventory);
    }

    public override void FixedRun(){
        if (task == null) return;

        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
