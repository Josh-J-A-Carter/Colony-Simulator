using UnityEngine;

public class Destroy__Complete : State {
    DestroyTask task => (DestroyTask) taskAgent.GetTask();

    [SerializeField]
    AnimationClip anim;

    ITaskAgent taskAgent;

    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        // Make sure we can actually complete the task lol
        if (task.IsConstructableTagPresent(ConstructableTag.HoneyBeeDestructable) == false) {
            CompleteState(false);
        }
    }

    public override void FixedRun(){
        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
