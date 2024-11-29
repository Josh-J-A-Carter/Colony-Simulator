using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour, TaskAgent {

    [SerializeField]
    State Idle, House, Forage;
    Animator animator;

    WorkerTask task;

    StateMachine stateMachine;

    void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, animator, null);
        }

        TaskManager.Instance.RegisterAgent(this);
    }

    public bool OfferTask(Task task) {
        if (task is WorkerTask workerTask) {
            this.task = workerTask;
            return true;
        } else return false;
    }

    public void SetTask(Task task) {
        if (task is WorkerTask workerTask) this.task = workerTask;
    }

    public Task GetTask() {
        return task;
    }

    void Update() {
        if (stateMachine.EmptyState()) DecideState();

        stateMachine.Run();
    }

    void FixedUpdate() {
        stateMachine.FixedRun();
    }

    void DecideState() {
        stateMachine.SetState(Idle);
    }
}
