using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nurse__Administer : State {
    NurseTask task => (NurseTask) taskAgent.GetTask();

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
