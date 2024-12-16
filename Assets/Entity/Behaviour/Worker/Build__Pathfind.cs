using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Build__Pathfind : State {
    Path path;
    BuildTask task => (BuildTask) taskAgent.GetTask();

    int step, stepsMax;

    static readonly int stepSpeed = 15;

    [SerializeField]
    AnimationClip anim;

    public ITaskAgent taskAgent;

    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        TryFindPath();
    }

    public override void FixedRun() {
        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step > stepsMax) {
            CompleteState();
            return;
        }

        if (success == false) TryFindPath();
    }

    void TryFindPath() {
        ReadOnlyCollection<Vector2Int> interior = task.GetExteriorPoints();

        // Find a path to one of them, if possible
        (path, _) = Pathfind.FindPathToOneOf(transform.position, interior.ToList(), p => p);

        if (path != null) {
            step = 0;
            stepsMax = path.Count * stepSpeed;
            return;
        }

        CompleteState(false);
    }

}
