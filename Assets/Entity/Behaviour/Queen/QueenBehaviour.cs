using System;
using UnityEngine;

public class QueenBehaviour : MonoBehaviour, ITaskAgent, IInformative, IEntity, ITargetable {
    
    [SerializeField]
    State idle, lay, eat, die, sting;
    Animator animator;
    Task task;
    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    InventoryManager inventory;
    HealthComponent healthComponent;
    bool isDead;

    GravityComponent gravity;

    String nameInfo;

    const int STING_COOL_OFF = 2;
    float beganStingCoolOff;

    public GameObject GetGameObject() {
        return gameObject;
    }

    public void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();
        inventory = GetComponent<InventoryManager>();
        healthComponent = GetComponent<HealthComponent>();
        gravity = GetComponent<GravityComponent>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, animator, stateMachine);
        }

        TaskManager.Instance.RegisterAgent(this);
    }

    public bool OfferTask(Task task) {
        if (task.IsQueenTask()) {
            this.task = task;
            return true;
        } else return false;
    }

    public void SetTask(Task task) {
        if (task == null) {
            this.task = null;
            stateMachine.ResetChildState();
            return;
        }

        if (task.IsQueenTask()) {
            this.task = task;
        } else throw new Exception("Cannot accept the task as it is not of type WorkerTask");

        stateMachine.ResetChildState();

    }
    
    public void CancelTask() {
        stateMachine.ResetChildState();
    }

    public Task GetTask() {
        return task;
    }


    public void Update() {
        stateMachine.Run();
    }

    public void FixedUpdate() {
        stateMachine.FixedRun();

        if (isDead) return;

        if (healthComponent.IsDead) {
            OnDeath();
            return;
        }

        if (stateMachine.EmptyState()) DecideState();
    }

    void DecideState() {
        // Low nutrition -> immediate action, if there is food available
        if (healthComponent.LowNutrition) {
            Resource resource = new Resource(ItemTag.Food);
            if (ResourceManager.Instance.Available(inventory, resource)) {
                stateMachine.SetChildState(eat);
                return;
            }
        }

        if (task == null) {
            // Eat (with less urgency)
            if (healthComponent.Nutrition <= 3 * healthComponent.MaxNutrition / 4) {
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

        else if (task is AttackTask) {
            if (beganStingCoolOff + STING_COOL_OFF < Time.time) {
                stateMachine.SetChildState(sting);
            }

            else {
                stateMachine.SetChildState(idle);
            }
        }
    }
    void OnDeath() {
        if (task != null) (this as ITaskAgent).CancelAssignment();

        stateMachine.SetChildState(die);
        isDead = true;

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
        root.AddChild(healthComponent.GetInfoBranch());

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

    public bool IsDead() {
        return healthComponent.IsDead;
    }

    public int Friendliness() {
        return 1;
    }

    public Vector2 GetPosition() {
        return transform.position;
    }

    public void Damage(uint amount, ITargetable attacker = null) {
        healthComponent.Damage(amount);

        if (attacker != null) {
            TaskManager.Instance.CreateTask(new AttackTask(attacker, TaskPriority.Critical));
        }
    }

    public void InitiateStingCoolOff() {
        beganStingCoolOff = Time.time;
        (this as ITaskAgent).CancelAssignment();
    }
}
