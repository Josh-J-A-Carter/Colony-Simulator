using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Harvest__Pathfind : State {


    Path path;
    ForageTask task => (ForageTask) taskAgent.GetTask();
    static readonly int stepSpeed = 15;

    [SerializeField]
    AnimationClip anim;

    ITaskAgent taskAgent;

    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        TryFindPath();
    }

    public override void FixedRun(){
        bool success = path.Increment();

        if (path.IsComplete()) {
            CompleteState();
            return;
        }

        if (success == false) TryFindPath();
    }

    void TryFindPath() {
        ReadOnlyCollection<Vector2Int> interior = task.GetInteriorPoints();

        // Find a path to one of them, if possible
        (path, _) = Pathfind.FindPathToOneOf(transform.position, interior.ToList(), p => p, oneTagFrom: new[]{ ConstructableTag.BeeTraversable });

        if (path != null) {
            path.Initialise(entity, stepSpeed);
            return;
        }

        CompleteState(false);
    }
}
