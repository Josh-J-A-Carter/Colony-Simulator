using UnityEngine;
using UnityEngine.Assertions;

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

    #if UNITY_EDITOR
        if (childState == null) throw new System.Exception("Child state is unassigned or null!");
    #endif

        // Make sure we change the state BEFORE calling OnChildExit, otherwise we might have an infinite recursion
        State oldState = this.childState;

        this.childState = childState;

        // Exit code down the branch
        oldState?.OnExitRecursive();

        activeSince = Time.time;
        this.childState?.OnEntry();
    }

    public void ResetChildState(bool success = true) {
        // Make sure we change the state BEFORE calling OnChildExit, otherwise we might have an infinite recursion
        State oldState = childState;

        childState = null;

        // Exit code down the branch
        oldState?.OnExitRecursive();
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
