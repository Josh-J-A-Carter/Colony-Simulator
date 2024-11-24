using UnityEngine;

public class StateMachine {

    public State currentState;

    public float activeSince;

    public float activeFor => Time.time - activeSince;

    public void SetState(State state) {
        if (state == currentState) return;

        // Exit code down the branch
        currentState?.OnExitRecursive();
        // Notify parents up the branch that we've exited down the branch
        currentState?.parent?.OnChildExit(currentState);

        activeSince = Time.time;

        currentState?.OnEnter();
    }

    public void ResetState() {
        // Exit code down the branch
        currentState?.OnExitRecursive();
        // Notify parents up the branch that we've exited down the branch
        currentState?.parent?.OnChildExit(currentState);

        currentState = null;
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
