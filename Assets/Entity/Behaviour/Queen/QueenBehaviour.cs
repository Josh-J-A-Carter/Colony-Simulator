using System;
using UnityEngine;

public class QueenBehaviour : MonoBehaviour, ITaskAgent, IInformative, IEntity, ILiving {
    
    [SerializeField]
    State idle, lay, eat, die;
    Animator animator;
    QueenTask task;
    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    InventoryManager inventory;
    public HealthComponent HealthComponent { get; private set; }
    public bool IsDead { get; private set; }

    GravityComponent gravity;

    String nameInfo;

    public GameObject GetGameObject() {
        return gameObject;
    }

    public void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();
        inventory = GetComponent<InventoryManager>();
        HealthComponent = GetComponent<HealthComponent>();
        gravity = GetComponent<GravityComponent>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, animator, stateMachine);
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
        if (IsDead) return;

        if (HealthComponent.IsDead) {
            OnDeath();
            return;
        }

        if (stateMachine.EmptyState()) DecideState();

        stateMachine.Run();
    }

    public void FixedUpdate() {
        stateMachine.FixedRun();
    }

    void DecideState() {
        // Low nutrition -> immediate action, if there is food available
        if (HealthComponent.LowNutrition) {
            Resource resource = new Resource(ItemTag.Food);
            if (ResourceManager.Instance.Available(inventory, resource)) {
                stateMachine.SetChildState(eat);
                return;
            }
        }

        if (task == null) {
            // Eat (with less urgency)
            if (HealthComponent.Nutrition <= 3 * HealthComponent.MaxNutrition / 4) {
                Resource resource = new Resource(ItemTag.Food);
                if (ResourceManager.Instance.Available(inventory, resource)) {
                    stateMachine.SetChildState(eat);
                    return;
                }
            }
            
            stateMachine.SetChildState(idle);
        }

        if (task is LayTask) {
            stateMachine.SetChildState(lay);
            return;
        }
    }
    void OnDeath() {
        if (task != null) (this as ITaskAgent).CancelAssignment();

        stateMachine.SetChildState(die);
        IsDead = true;

        inventory.EmptyInventory();
        inventory.DisablePassiveProduction();

        gravity.Enable();
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


    #if UNITY_EDITOR
        InfoBranch taskCategory = new InfoBranch("Task Information");
        root.AddChild(taskCategory);

        InfoLeaf stateProperty = new InfoLeaf("State", DeepestChildState() + "");
        taskCategory.AddChild(stateProperty);
    #endif

        // Health
        root.AddChild(HealthComponent.GetInfoBranch());

        // Inventory
        InfoBranch inventoryCategory = inventory.GetInfoTree();
        root.AddChild(inventoryCategory);

        return root;
    }

    State DeepestChildState() {
        State curr = currentState;

        if (curr == null) return curr;

        while (curr.stateMachine.childState != null) curr = curr.stateMachine.childState;

        return curr;
    }
}
