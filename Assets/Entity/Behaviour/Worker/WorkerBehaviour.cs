using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour, TaskAgent, IInformative, Entity {

    [SerializeField]
    State Idle, Build, Nurse, Tidy;
    Animator animator;

    WorkerTask task;

    InventoryManager inventory;

    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    String nameInfo;

    public GameObject GetGameObject() {
        return gameObject;
    }

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
        if (task is Consumer consumer && !AreResourcesAvailable(consumer)) return false;

        // Make sure pathfinding to the task is possible, if applicable
        if (task is ILocative locative && !IsPathAvailable(locative)) return false;

        if (task is WorkerTask workerTask) {
            this.task = workerTask;
            // if (currentState == Idle) stateMachine.ResetChildState();
            return true;
        } else return false;
    }

    bool IsPathAvailable(ILocative locative) {
        ReadOnlyCollection<Vector2Int> exterior = locative.GetExteriorPoints();
        foreach (Vector2Int destination in exterior) {
            Path path = Pathfind.FindPath(transform.position, destination);
            if (path != null) return true;
        }
            
        return false;
    }

    bool AreResourcesAvailable(Consumer consumer) {
        // Try find locations to get each resource
        foreach ((Item item, uint quantity) in consumer.GetRequiredResources()) {
            // Inventory has it? Go to next resource
            if (inventory.Has(item, quantity)) continue;

            // ItemEntities have it?
            // if (EntityManager.Instance.FindItemEntities(item, quantity, out _)) continue;

            // Storage has it?
            // if (TileManager.Instance.FindItemInStorage(item, quantity, out _)) continue;

            // Nothing in the nest has it readily accessible
            return false;
        }

        return true;
    }

    public void SetTask(Task task) {
        if (task == null) {
            this.task = null;
            stateMachine.ResetChildState();
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
            
            // If inventory full or random items around, AND available storage, then tidy
            bool invFull = inventory.RemainingCapacity() <= inventory.MaxCapacity() / 5;
            ReadOnlyCollection<ItemEntity> itemEntities = EntityManager.Instance.GetItemEntities();
            List<(Vector2Int, IStorage, Dictionary<String, object>)> storage = TileManager.Instance.FindAvailableStorage();

            if ((invFull || itemEntities.Count > 0) && storage.Count > 0) {
                stateMachine.SetChildState(Tidy);
            }

            else {
                stateMachine.SetChildState(Idle);
            }

            return;
        }

        if (task is BuildTask) {
            stateMachine.SetChildState(Build);
        }

        else if (task is NurseTask) {
            stateMachine.SetChildState(Nurse);
        }

        else {
            throw new Exception("Unknown/incompatible task type");
        }
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

        // Task
        InfoBranch taskCategory = new InfoBranch("Task information");
        root.AddChild(taskCategory);

        if (task == null) {
            InfoLeaf currentTaskProperty = new InfoLeaf("Current task", "None", "As this bee is not assigned a particular task, it may instead be tidying or idling");
            taskCategory.AddChild(currentTaskProperty);
        } else {
            foreach (InfoNode node in task.GetInfoTree().GetChildren()) {
                taskCategory.AddChild(node);
            }
        }

        // Inventory
        InfoBranch inventoryCategory = inventory.GetInfoTree();
        root.AddChild(inventoryCategory);

        return root;
    }
}