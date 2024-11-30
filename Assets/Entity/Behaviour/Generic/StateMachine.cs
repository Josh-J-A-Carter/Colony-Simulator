using UnityEngine;

public class StateMachine {

    public State childState;

    public readonly State state;

    public float activeSince;

    public float activeFor => Time.time - activeSince;

    public StateMachine() {}
    public StateMachine(State state) {
        this.state = state;
    }

    public void SetChildState(State childState) {
        if (this.childState == childState) return;

        // Make sure we change the state BEFORE calling OnChildExit, otherwise we might have an infinite recursion
        State oldState = this.childState;

        this.childState = childState;

        // Exit code down the branch
        oldState?.OnExitRecursive();
        // Notify parents up the branch that we've exited down the branch
        state?.OnChildExit(oldState);

        activeSince = Time.time;
        this.childState?.OnEntry();
    }

    public void ResetChildState(bool success = true) {
        // Make sure we change the state BEFORE calling OnChildExit, otherwise we might have an infinite recursion
        State oldState = childState;

        childState = null;

        // Exit code down the branch
        oldState?.OnExitRecursive();
        // Notify parents up the branch that we've exited down the branch
        state?.OnChildExit(oldState, success);
    }

    public bool EmptyState() {
        return childState == null;
    }

    public void Run() {
        childState?.RunRecursive();
    }

    public void FixedRun() {
        childState?.FixedRunRecursive();
    }

}
