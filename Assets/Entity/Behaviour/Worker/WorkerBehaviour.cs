using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour, TaskAgent, Informative {

    [SerializeField]
    State Idle, Hive, Forage;
    Animator animator;

    WorkerTask task;

    const uint MAX_INVENTORY_CAPACITY = 20;
    Inventory inventory;

    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    String nameInfo;

    public void Start() {
        stateMachine = new StateMachine();
        inventory = new Inventory(MAX_INVENTORY_CAPACITY);

        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, this, animator, stateMachine, inventory);
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
        if (task == null) {
            this.task = null;
            return;
        }

        if (task is WorkerTask workerTask) {
            this.task = workerTask;
        }
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

        switch (task.category) {
            case WorkerTaskType.Hive: {
                stateMachine.SetChildState(Hive);
                return;
            }
        }

        stateMachine.SetChildState(Idle);
    }

    public Sprite GetPreviewSprite() {
        throw new NotImplementedException();
    }

    public String GetName() {
        return nameInfo;
    }

    public void SetName(String name) {
        nameInfo = name;
    }

    public String GetDescription() {
        return "Worker bee; Girl power";
    }

    public InfoBranch GetInfoTree(object _ = null) {
        InfoBranch root = new InfoBranch(String.Empty);

        // Generic
        InfoBranch genericCategory = new InfoBranch("Generic Properties");
        root.AddChild(genericCategory);

        InfoLeaf typeProperty = new InfoLeaf("Type", "Worker Honey Bee (Entity)");
        genericCategory.AddChild(typeProperty);

        InfoLeaf nameProperty = new InfoLeaf("Name", nameInfo);
        genericCategory.AddChild(nameProperty);
        
        // Inventory
        InfoBranch inventoryCategory = inventory.GetInfoTree();
        root.AddChild(inventoryCategory);

        // Task


        return root;
    }

    public InfoType GetInfoType() {
        return InfoType.Entity;
    }
}