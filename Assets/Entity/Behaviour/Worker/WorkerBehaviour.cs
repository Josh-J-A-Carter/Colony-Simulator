using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour, TaskAgent, Informative {

    [SerializeField]
    State Idle, Build, Nurse, Forage;
    Animator animator;

    WorkerTask task;

    InventoryManager inventory;

    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    String nameInfo;

    public void Start() {
        stateMachine = new StateMachine();
        
        inventory = GetComponent<InventoryManager>();
        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, this, animator, stateMachine, inventory);
        }

        TaskManager.Instance.RegisterAgent(this);
    }

    public bool OfferTask(Task task) {
        // *Prefer* continuing with the current task
        if (this.task != null) return false;

        // Check resource requirements. If they aren't met, can't help
        if (task is Consumer consumer) {

            bool missingRequirement = false;
            foreach ((Item item, uint quantity) in consumer.GetRequiredResources()) {
                if (!inventory.Has(item, quantity)) missingRequirement = true;
            }

            if (missingRequirement && !consumer.HasAllocation()) return false;
        }

        // Make sure pathfinding to the task is possible, if applicable
        if (task is Locative locative) {
            bool foundPath = false;
            ReadOnlyCollection<Vector2Int> exterior = locative.GetExteriorPoints();
            foreach (Vector2Int destination in exterior) {
                Path path = Pathfind.FindPath(transform.position, destination);
                if (path != null) foundPath = true;
            }
            
            if (!foundPath) return false;
        }

        if (task is WorkerTask workerTask) {
            this.task = workerTask;
            stateMachine.ResetChildState();
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
        } else throw new Exception("Cannot accept the task as it is not of type WorkerTask");

        stateMachine.ResetChildState();
    }

    public void OnTaskCancellation() {
        task = null;
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

        switch (task.category) {
            case WorkerTaskType.Hive: {
                stateMachine.SetChildState(Build);
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