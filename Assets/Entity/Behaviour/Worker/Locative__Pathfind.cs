using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Locative__Pathfind : State {
    Path path;
    ILocative task => (ILocative) taskAgent.GetTask();

    static readonly int stepSpeed = 15;

    [SerializeField]
    AnimationClip anim;

    ITaskAgent taskAgent;

    bool targetInterior;

    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }

    public void TargetInteriorPoints() {
        targetInterior = true;
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        TryFindPath();
    }

    public override void FixedRun() {
        bool success = path.Increment();

        if (path.IsComplete()) {
            CompleteState();
            return;
        }

        if (success == false) TryFindPath();
    }

    void TryFindPath() {
        ReadOnlyCollection<Vector2Int> points;
        if (targetInterior) points = task.GetInteriorPoints();
        else points = task.GetExteriorPoints();

        // Find a path to one of them, if possible
        (path, _) = Pathfind.FindPathToOneOf(transform.position, points.ToList(), p => p, oneTagFrom: new[]{ ConstructableTag.BeeTraversable });

        if (path != null) {
            path.Initialise(entity, stepSpeed);
            return;
        }

        CompleteState(false);
    }

}
