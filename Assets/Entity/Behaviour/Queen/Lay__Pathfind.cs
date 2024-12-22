using System;
using UnityEngine;

public class Lay__Pathfind : State {
    Path path;
    LayTask task => (LayTask) taskAgent.GetTask();

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
        Vector2Int destination = task.GetLocation();
        
        Vector2 pos = entity.transform.position;
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));

        path = Pathfind.FindPath(gridPos, destination);

        // We couldn't find a path to the task location :(
        if (path == null) {
            CompleteState(false);
            return;
        }

        path.Initialise(entity, stepSpeed);
    }
}
