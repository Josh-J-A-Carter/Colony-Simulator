using System;
using System.Linq;
using UnityEngine;

public abstract class State : MonoBehaviour {

    public StateMachine parent;
    public StateMachine stateMachine;
    protected State child => stateMachine.childState;

    public TaskAgent taskAgent;
    public Animator animator;
    public GameObject entity;
    public Task task;

    public void Setup(GameObject entity, TaskAgent taskAgent, Animator animator, StateMachine parent) {
        stateMachine = new StateMachine(this);

        this.entity = entity;
        this.taskAgent = taskAgent;
        this.animator = animator;
        this.parent = parent;

        // Recursively set up child states, if present
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(entity, taskAgent, animator, stateMachine);
        }
    }

    public virtual void OnEntry() {}


    public void OnExitRecursive() {
        OnExit();

        stateMachine.ResetChildState();
    }

    public virtual void OnExit() {}

    public virtual void OnChildExit(State exitingChild) {}


    public void CompleteState() {
        parent.ResetChildState();
    }

    public void RunRecursive() {
        this.Run();
        stateMachine?.Run();
    }

    public virtual void Run() {}

    public void FixedRunRecursive() {
        this.FixedRun();
        stateMachine?.FixedRun();
    }

    public virtual void FixedRun() {}
}
