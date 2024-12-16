using UnityEngine;

public class Harvest__Collect : State {
    ForageTask task => (ForageTask) taskAgent.GetTask();

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
