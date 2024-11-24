using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeanderState : State {

    [SerializeField]
    AnimationClip animation;

    public override void OnEnter() {
        this.animator.Play(animation.name);

        // Choose a destination (within a given radius)
    }

    public override void OnExit() {}

    public override void FixedRun() {
        // Check path still valid
            // yes -> 
                // Follow path to destination

                // Remember to flip the character's sprite as needed

            // no ->
                // Choose a new destination
    }

    public override void Run() {
        
    }

}
