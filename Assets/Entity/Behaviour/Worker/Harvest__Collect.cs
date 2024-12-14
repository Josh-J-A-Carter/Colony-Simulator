using UnityEngine;

public class Harvest__Collect : State {
    ForageTask task => (ForageTask) taskAgent.GetTask();

    [SerializeField]
    AnimationClip anim;

    public override void OnEntry() {
        animator.Play(anim.name);
    }

    public override void FixedRun(){
        if (task == null) return;

        task.IncrementProgress();
        if (task.IsComplete()) {
            inventory.Give(task.GetRewardItems());
            CompleteState();
        }
    }
}
