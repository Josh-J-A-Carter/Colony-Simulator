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
    public InventoryManager inventory;

    public void Setup(GameObject entity, TaskAgent taskAgent, Animator animator, StateMachine parent, InventoryManager inventory = null) {
        stateMachine = new StateMachine(this);

        this.entity = entity;
        this.taskAgent = taskAgent;
        this.animator = animator;
        this.parent = parent;
        this.inventory = inventory;

        // Recursively set up child states, if present
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(entity, taskAgent, animator, stateMachine, inventory);
        }
    }

    public virtual void OnEntry() {}


    public void OnExitRecursive() {
        OnExit();

        stateMachine.ResetChildState();
    }

    public virtual void OnExit() {}

    public virtual void OnChildExit(State exitingChild, bool success = true) {}


    public void CompleteState(bool success = true) {
        parent.ResetChildState(success: success);

        parent.state?.OnChildExit(this, success);
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
