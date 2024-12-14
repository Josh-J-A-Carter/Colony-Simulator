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
        ReadOnlyCollection<Vector2Int> exterior = task.GetExteriorPoints();

        Vector2 pos = entity.transform.position;
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));
        List<Vector2Int> orderedExterior = exterior.OrderBy(tile => Math.Pow(tile.x - gridPos.x, 2) + Math.Pow(tile.y - gridPos.y, 2)).ToList();

        foreach (Vector2Int destination in orderedExterior) {
            Path possiblePath = Pathfind.FindPath(gridPos, destination);

            if (possiblePath == null) continue;

            path = possiblePath;
            break;
        }

        // We couldn't find a path to the task location :(
        if (path == null) {
            CompleteState(false);
            return;
        }

        step = 0;
        stepsMax = path.Count * stepSpeed;
    }

}
