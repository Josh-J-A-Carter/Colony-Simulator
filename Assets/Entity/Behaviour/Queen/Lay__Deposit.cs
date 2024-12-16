using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lay__Deposit : State {
    LayTask task => (LayTask) taskAgent.GetTask();

    
    [SerializeField]
    AnimationClip anim;

    ITaskAgent taskAgent;
    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);
    }

    public override void FixedRun(){
        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
