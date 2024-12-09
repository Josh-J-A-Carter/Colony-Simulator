using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Nurse__Pathfind : State {


    Path path;
    NurseTask task => (NurseTask) taskAgent.GetTask();

    int step, stepsMax;

    static readonly int stepSpeed = 15;

    [SerializeField]
    AnimationClip anim;

    public override void OnEntry() {
        animator.Play(anim.name);

        TryFindPath();
    }

    public override void FixedRun(){
        int currentX = (int) Math.Floor(entity.transform.position.x);
        int currentY = (int) Math.Floor(entity.transform.position.y);

        Vector2Int current = new Vector2Int(currentX, currentY);

        if (path.IsValidFrom(current)) {
            
            // Linearly interpolate to next point
            Vector2 nextPoint = path.LinearlyInterpolate(step, stepsMax);
            Vector2 translation = nextPoint - (Vector2) entity.transform.position;
            entity.transform.Translate(translation);

            /// Remember to flip the character's sprite as needed
            int sign = Math.Sign(translation.x);
            if (sign != 0) entity.transform.localScale = new Vector3(sign, 1, 1);

            step += 1;

            // Once we reach the end of the path, the state is completed
            if (step > stepsMax) CompleteState();

        } else TryFindPath();
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
