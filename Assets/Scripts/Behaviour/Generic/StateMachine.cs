using UnityEngine;

public class StateMachine {

    public State currentState;

    public float activeSince;

    public float activeFor => Time.time - activeSince;

    public void SetState(State state) {
        if (state == currentState) return;

        // Make sure we change the state BEFORE calling OnChildExit, otherwise we might have an infinite recursion
        State oldState = currentState;

        currentState = state;

        // Exit code down the branch
        oldState?.OnExitRecursive();
        // Notify parents up the branch that we've exited down the branch
        oldState?.parent?.OnChildExit(oldState);

        activeSince = Time.time;
        currentState?.OnEnter();
    }

    public void ResetState() {
        // Make sure we change the state BEFORE calling OnChildExit, otherwise we might have an infinite recursion
        State oldState = currentState;

        currentState = null;

        // Exit code down the branch
        oldState?.OnExitRecursive();
        // Notify parents up the branch that we've exited down the branch
        oldState?.parent?.OnChildExit(oldState);
    }

    public bool EmptyState() {
        return currentState == null;
    }

    public void Run() {
        currentState?.RunRecursive();
    }

    public void FixedRun() {
        currentState?.FixedRunRecursive();
    }

}
