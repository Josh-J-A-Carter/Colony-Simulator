using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StallState : State {

    [SerializeField]
    AnimationClip animation;

    // Min & max times we will remain in this state
    static readonly float minStallTime = 1;
    static readonly float maxStallTime = 3;

    // Time to remain in the state (will be chosen randomly when the state is next entered)
    float stallTime;

    public override void OnEnter() {
        this.animator.Play(animation.name);

        // Choose a random amount of time to stall
        stallTime = Random.Range(minStallTime, maxStallTime);
    }

    public override void Run() {
        if (parent.stateMachine.activeFor > stallTime) {
            CompleteState();
        }
    }

}
