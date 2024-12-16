using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lay__Pathfind : State {
    Path path;
    LayTask task => (LayTask) taskAgent.GetTask();

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
        Vector2Int destination = task.GetLocation();
        
        Vector2 pos = entity.transform.position;
        Vector2Int gridPos = new Vector2Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y));

        path = Pathfind.FindPath(gridPos, destination);

        // We couldn't find a path to the task location :(
        if (path == null) CompleteState();

        step = 0;
        stepsMax = path.Count * stepSpeed;
    }
}
