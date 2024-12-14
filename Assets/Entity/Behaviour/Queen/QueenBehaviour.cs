using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBehaviour : MonoBehaviour, TaskAgent, IInformative, Entity {
    
    [SerializeField]
    State Idle, Lay;
    Animator animator;
    QueenTask task;
    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    String nameInfo;

    public GameObject GetGameObject() {
        return gameObject;
    }

    public void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, this, animator, stateMachine);
        }

        TaskManager.Instance.RegisterAgent(this);
    }

    public bool OfferTask(Task task) {
        if (task is QueenTask queenTask) {
            this.task = queenTask;
            return true;
        } else return false;
    }

    public void SetTask(Task task) {
        if (task == null) {
            this.task = null;
            return;
        }

        if (task is QueenTask queenTask) {
            this.task = queenTask;
        }
    }
    
    public void CancelTask() {
        stateMachine.ResetChildState();
    }

    public Task GetTask() {
        return task;
    }



    public void Update() {
        if (stateMachine.EmptyState()) DecideState();

        stateMachine.Run();
    }

    public void FixedUpdate() {
        stateMachine.FixedRun();
    }

    void DecideState() {
        if (task == null) {
            stateMachine.SetChildState(Idle);
            return;
        }

        if (task is LayTask layTask) {
            stateMachine.SetChildState(Lay);
            return;
        }

        stateMachine.SetChildState(Idle);
    }

    public string GetName() {
        return nameInfo;
    }

    public void SetName(String name) {
        nameInfo = name;
    }

    public string GetDescription() {
        throw new System.NotImplementedException();
    }

    public InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new InfoBranch(String.Empty);

        // Generic
        InfoBranch genericCategory = new InfoBranch("Generic Properties");
        root.AddChild(genericCategory);

        InfoLeaf typeProperty = new InfoLeaf("Type", "Queen Honey Bee (Entity)");
        genericCategory.AddChild(typeProperty);

        InfoLeaf nameProperty = new InfoLeaf("Name", nameInfo);
        genericCategory.AddChild(nameProperty);

        return root;
    }
}
