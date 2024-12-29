using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour, ITaskAgent, IInformative, IEntity, ITargetable {

    [SerializeField]
    State idle, build, nurse, tidy, forage, ferment, eat, die, sting, destroy;
    Animator animator;

    Task task;
    InventoryManager inventory;
    HealthComponent healthComponent;
    bool isDead;

    GravityComponent gravity;

    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    String nameInfo;

    const int STING_COOL_OFF = 4;
    float beganStingCoolOff;

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

    public HealthComponent GetHealthComponent() {
        return healthComponent;
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
            child.GetComponent<State>()?.Setup(gameObject, animator, stateMachine);
        }

        TaskManager.Instance.RegisterAgent(this);
    }

    public bool OfferTask(Task task) {
        if (isDead) return false;

        // *Prefer* continuing with the current task
        if (this.task != null) return false;

        // Check resource requirements. If they aren't met, can't help
        if (task is IConsumer consumer && !ResourceManager.Instance.Available(inventory, consumer.GetRequiredResources())) return false;

        // Make sure pathfinding to the task is possible, if applicable
        if (task is ILocative locative && !IsPathAvailable(locative)) return false;

        // Bee needs to be able to destroy the target constructable!
        if (task is DestroyTask destroyTask && !destroyTask.IsConstructableTagPresent(ConstructableTag.HoneyBeeDestructable)) return false;

        if (task.IsWorkerTask()) {
            this.task = task;
            return true;
        } else return false;
    }

    bool IsPathAvailable(ILocative locative) {
        ReadOnlyCollection<Vector2Int> exterior = locative.GetExteriorPoints();
        foreach (Vector2Int destination in exterior) {
            Path path = Pathfind.FindPath(transform.position, destination, oneTagFrom: new[]{ ConstructableTag.BeeTraversable });
            if (path != null) return true;
        }
        
        return false;
    }

    public void SetTask(Task task) {
        if (task == null) {
            this.task = null;
            stateMachine.ResetChildState();
            return;
        }

        if (task.IsWorkerTask()) {
            this.task = task;
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

        // Low nutrition -> immediate action, provided there is food available
        if (healthComponent.LowNutrition || healthComponent.LowHealth) {
            Resource resource = new Resource(ItemTag.Food);
            if (ResourceManager.Instance.Available(inventory, resource)) {
                stateMachine.SetChildState(eat);
                return;
            }
        }

        // Full inventory -> immediate action, provided storage available
        if (inventory.RemainingCapacity() == 0) {
            List<(Vector2Int, IStorage, Dictionary<String, object>)> itemStorage = TileManager.Instance.FindAvailableStorage();
            if (itemStorage.Count > 0) {
                stateMachine.SetChildState(tidy);
                return;
            }
        }

        if (task == null) {
            /// Fermenting

            // Stuff to store
            Resource resource = new Resource(ItemTag.Fermentable);
            List<(Vector2Int, BroodComb, Dictionary<String, object>)> fermentableStorage = TileManager.Instance.QueryTileEntities<BroodComb>(
                tuple => tuple.Item2.CanStoreFermentable(tuple.Item3)
            );
            if (ResourceManager.Instance.Available(inventory, resource) && fermentableStorage.Count > 0) {
                stateMachine.SetChildState(ferment);
                return;
            }

            // Stuff to collect
            fermentableStorage = TileManager.Instance.QueryTileEntities<BroodComb>(
                tuple => tuple.Item2.FermentablesReady(tuple.Item3)
            );
            if (fermentableStorage.Count > 0) {
                stateMachine.SetChildState(ferment);
                return;
            }

            /// Tidying
            bool invFull = inventory.RemainingCapacity() <= inventory.MaxCapacity() / 5;
            ReadOnlyCollection<ItemEntity> itemEntities = EntityManager.Instance.GetItemEntities();
            List<(Vector2Int, IStorage, Dictionary<String, object>)> itemStorage = TileManager.Instance.FindAvailableStorage();

            if ((invFull || itemEntities.Count > 0) && itemStorage.Count > 0) {
                stateMachine.SetChildState(tidy);
                return;
            }

            // Eat (with less urgency)
            if (healthComponent.Nutrition <= 3 * healthComponent.MaxNutrition / 4 
                || healthComponent.Health <= 3 * healthComponent.MaxHealth / 4) {
                resource = new Resource(ItemTag.Food);
                if (ResourceManager.Instance.Available(inventory, resource)) {
                    stateMachine.SetChildState(eat);
                    return;
                }
            }
            
            /// Idle
            stateMachine.SetChildState(idle);
            return;
        }

        if (task is BuildTask) {
            stateMachine.SetChildState(build);
        }

        else if (task is NurseTask) {
            stateMachine.SetChildState(nurse);
        }

        else if (task is ForageTask) {
            stateMachine.SetChildState(forage);
        }

        else if (task is AttackTask attack) {
            if (beganStingCoolOff + STING_COOL_OFF < Time.time && attack.GetTarget().CanTarget()) {
                stateMachine.SetChildState(sting);
            }

            else {
                stateMachine.SetChildState(idle);
            }
        }

        else if (task is DestroyTask destroyTask) {
            stateMachine.SetChildState(destroy);
        }
        
        else {
        #if UNITY_EDITOR
            throw new Exception("Unknown/incompatible task type");
        #else
            stateMachine.SetChildState(idle);
        #endif
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
        InfoBranch taskCategory = new InfoBranch("Task Information");
        root.AddChild(taskCategory);

        if (task == null) {
            InfoLeaf currentTaskProperty = new InfoLeaf("Current task", "None", "As this bee is not assigned a particular task, it may instead be tidying or idling");
            taskCategory.AddChild(currentTaskProperty);
        } else {
            foreach (InfoNode node in task.GetGenericInfoTree().GetChildren()) {
                taskCategory.AddChild(node);
            }
        }
    
    #if UNITY_EDITOR
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

    public int Friendliness() {
        return 1;
    }

    public void Damage(uint amount, ITargetable attacker = null) {
        healthComponent.Damage(amount);

        if (attacker != null) {
            TaskManager.Instance.CreateTask(new AttackTask(attacker, TaskPriority.Urgent));
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

    public Vector2 GetPosition() {
        return transform.position;
    }

    public bool CanTarget() {
        return !healthComponent.IsDead;
    }

    public bool IsDead() {
        return healthComponent.IsDead;
    }

    public void InitiateStingCoolOff() {
        beganStingCoolOff = Time.time;
        (this as ITaskAgent).CancelAssignment();
    }
}