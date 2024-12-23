using System;
using UnityEngine;

public class DroneBehaviour : MonoBehaviour, IInformative, IEntity, ITargetable {
    
    [SerializeField]
    State idle, eat, die;
    protected Animator animator;
    protected StateMachine stateMachine;

    State currentState => stateMachine.childState;
    
    InventoryManager inventory;
    HealthComponent healthComponent;
    bool isDead;

    GravityComponent gravity;

    String nameInfo;

    Material material;
    float beganHurtTime;
    bool hurt;
    const float MAX_HURT_TIME = 0.75f;
    const String RED_TINT = "_REDTINT_ON";

    const String OUTLINE = "_OUTLINE_ON";


    public GameObject GetGameObject() {
        return gameObject;
    }

    public void SetOutline() {
        material.EnableKeyword(OUTLINE);
    }

    public void ResetOutline() {
        material.DisableKeyword(OUTLINE);
    }

    public void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();
        inventory = GetComponent<InventoryManager>();
        healthComponent = GetComponent<HealthComponent>();
        gravity = GetComponent<GravityComponent>();
        material = GetComponent<Renderer>().material;

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, animator, stateMachine);
        }
    }

    public void Update() {
        HideHurt();

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
        if (healthComponent.Nutrition <= 3 * healthComponent.MaxNutrition / 4) {
            Resource resource = new Resource(ItemTag.Food);
            if (ResourceManager.Instance.Available(inventory, resource)) {
                stateMachine.SetChildState(eat);
                return;
            }
        }

        stateMachine.SetChildState(idle);
    }

    void OnDeath() {
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

        InfoLeaf typeProperty = new InfoLeaf("Type", "Drone Honey Bee (Entity)");
        genericCategory.AddChild(typeProperty);

        InfoLeaf nameProperty = new InfoLeaf("Name", nameInfo);
        genericCategory.AddChild(nameProperty);

        
        // Task
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
            TaskManager.Instance.CreateTask(new AttackTask(attacker, TaskPriority.Important));
        }
        ShowHurt();
    }
    
    void ShowHurt() {
        // Visual hurt indication
        hurt = true;
        beganHurtTime = Time.time;
        material.EnableKeyword(RED_TINT);
    }

    void HideHurt() {
        if (hurt == false || beganHurtTime + MAX_HURT_TIME > Time.time) return;

        material.DisableKeyword(RED_TINT);
        hurt = false;
    }
}
