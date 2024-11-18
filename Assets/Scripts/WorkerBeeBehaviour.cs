using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerBeeBehaviour : MonoBehaviour
{

    [SerializeField]
    Animator animator;

    enum State {
        Idle,
        Fly
    };

    State currentState = State.Idle;

    int timer = 0;
    int timerMax = 300;

    // void Start() {}
    void FixedUpdate() {
        timer += 1;

        if (timer >= timerMax) {
            timer = 0;
            ChangeState(currentState == State.Idle ? State.Fly : State.Idle);
        }
    }

    void ChangeState(State newState) {
        // Don't want to restart animations
        if (currentState == newState) return;

        // Want the benefits of type safety - so using an enum, not a static class.
        // But this may have a performance impact since it uses reflection...
        String newStateName = Enum.GetName(typeof(State), newState);

        animator.Play(newStateName);
        currentState = newState;
    }

}
