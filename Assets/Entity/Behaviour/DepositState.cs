using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepositState : State {
    LayTask task => (LayTask) taskAgent.GetTask();

    
    [SerializeField]
    AnimationClip anim;

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
