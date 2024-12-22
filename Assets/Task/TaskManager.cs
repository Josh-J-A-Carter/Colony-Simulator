using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour {

    const int ASSIGNMENT_RATE = 25;
    public static TaskManager Instance { get; private set; }


    int tick;

    LocativeTaskStore locativeTaskStore;

    List<Task> pendingCompletionTasks;
    List<Task> pendingAdditionTasks;

    List<TaskRule> taskRules;

    List<Task> tasks;
    List<ITaskAgent> assignedAgents, unassignedAgents;

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        tick = 0;

        locativeTaskStore = new();

        pendingCompletionTasks = new();
        pendingAdditionTasks = new();

        taskRules = new();

        tasks = new();
        assignedAgents = new();
        unassignedAgents = new();
    }

    public void FixedUpdate() {

        // Refresh task rules; give them an opportunity to cancel tasks, create new ones, etc.
        RefreshTaskRules();

        // Check for tasks that should be completed early
        EarlyTaskCompletion();

        // Deal with the queue of tasks that have been marked as complete
        ClearPendingCompletion();

        // Deal with the queue of tasks that are waiting to be added
        ClearPendingAddition();

        // 
        // To do: Deal with urgent tasks or something
        // 


        // Deal with agents that currently are not assigned a task
        tick += 1;

        if (tick == ASSIGNMENT_RATE) {
            tick = 0;
            OccupyUnassignedAgents();
        }
    }

    void RefreshTaskRules() {
        foreach (TaskRule rule in taskRules) rule.Refresh();
    }

    void EarlyTaskCompletion() {
        foreach (Task task in tasks) if (task.EarlyCompletion()) MarkComplete(task);
    }

    void OccupyUnassignedAgents() {
        for (int i = 0 ; i < unassignedAgents.Count ; i += 1) {
            ITaskAgent agent = unassignedAgents[i];
            SortTasksByUrgency();

            foreach (Task task in tasks) {
                if (agent.OfferTask(task)) {
                    assignedAgents.Add(agent);
                    unassignedAgents.RemoveAt(i);
                    task.IncrementAssignment();
                    i -= 1;
                    break;
                }
            }
        }
    }

    void SortTasksByUrgency() {
        tasks.Sort((t1, t2) => {
            if (t1.priority > t2.priority) return 1;
            if (t1.priority < t2.priority) return -1;

            if (t1.assignment > t2.assignment) return 1;
            if (t1.assignment < t2.assignment) return -1;

            if (t1.creationTime > t2.creationTime) return 1;
            if (t1.creationTime < t2.creationTime) return -1;

            return 0;
        });
    }

    void ClearPendingCompletion() {
        foreach (Task task in pendingCompletionTasks) {
            
            // Tell the task that it is complete
            task.OnCompletion();

            // Remove the task from the list
            tasks.Remove(task);

            // Reset all those agents whose task is set to this one
            for (int i = 0 ; i < assignedAgents.Count ; i += 1) {
                ITaskAgent agent = assignedAgents[i];

                if (agent.GetTask() != task) continue;

                agent.SetTask(null);
                unassignedAgents.Add(agent);
                assignedAgents.RemoveAt(i);
                i -= 1;
            }

            // Unset locative task store!
            if (task is ILocative locativeTask) locativeTaskStore.UnsetTask(locativeTask);
        }

        pendingCompletionTasks.Clear();
    }

    void ClearPendingAddition() {
        List<Task> toConfirm = new List<Task>();

        foreach (Task task in pendingAdditionTasks) {
            bool success = ResolveConflicts(task);

            if (success) {
                // Add the task to the list, & add to the locative task store;
                // Can't quite set it to be confirmed yet, as this will remove the pending status,
                // allowing other pending tasks (added at the same time) to override this,
                // when in reality they have the same age
                tasks.Add(task);
                toConfirm.Add(task);
                if (task is ILocative locativeTask) locativeTaskStore.SetTask(locativeTask);
            }
        }
        
        // Need to add them to a confirmation queue; two pending tasks might be added at the same time and overlap
        foreach (Task task in toConfirm) task.Confirm();

        pendingAdditionTasks.Clear();
    }

    bool ResolveConflicts(Task task) {
        if (task.MustAbort()) return false;

        if (task is ILocative locative) {
            List<Task> conflicts = locativeTaskStore.GetConflictingTasks(locative);

            // Look for a reason to cancel the new task (i.e. conflicts with another pending task)
            foreach (Task conflict in conflicts) if (conflict.IsPendingConfirmation()) return false;

            // No other pending tasks conflict with this one; therefore, all the other
            // conflicting tasks must be older than this one, and can thus be destroyed
            foreach (Task conflict in conflicts) CancelTask(conflict);
        }
 
        return true;
    }

    public void CancelTask(Task task) {
        
        // Remove the task from the list
        if (tasks.Remove(task) == false) return;
        
        // Tell the task that it has been cancelled
        task.OnCancellation();

        // If applicable, de-allocate the task's resources (i.e. items needed for its completion)
        if (task is IConsumer consumer) Deallocate(consumer);

        // Reset all those agents whose task is set to this one
        for (int i = 0 ; i < assignedAgents.Count ; i += 1) {
            if (assignedAgents[i].GetTask() != task) continue;

            assignedAgents[i].OnTaskCancellation();
            unassignedAgents.Add(assignedAgents[i]);
            assignedAgents.RemoveAt(i);
            i -= 1;
        }
        
        // Unset locative task store!
        if (task is ILocative locativeTask) locativeTaskStore.UnsetTask(locativeTask);
    }

    public void Allocate(IConsumer consumer, InventoryManager inventory) {
        if (consumer == null || !inventory) return;

        if (consumer.HasAllocation()) return;

        if (!inventory.HasResources(consumer.GetRequiredResources().ToList())) return;

        consumer.Allocate(inventory, inventory.TakeResources(consumer.GetRequiredResources().ToList()));        
    }

    void Deallocate(IConsumer consumer) {
        if (!consumer.HasAllocation()) return;

        (InventoryManager inventory, List<(Item, uint)> allocation) = consumer.Deallocate();

        if (inventory) {
            inventory.Give(allocation);
            return;
        }

        if (allocation == null) return;

        Vector2Int pos = consumer.GetDefaultDeallocationPosition();
        foreach ((Item item, uint quantity) in allocation) {
            EntityManager.Instance.InstantiateItemEntity(pos, item, quantity);
        }
    }

    public void RegisterAgent(ITaskAgent agent) {
        unassignedAgents.Add(agent);
    }

    public void DeregisterAgent(ITaskAgent agent) {
        if (unassignedAgents.Remove(agent)) return;

        agent.GetTask().DecrementAssignment();
        agent.SetTask(null);
        assignedAgents.Remove(agent);
    }

    public void UnassignAgent(ITaskAgent agent) {
        // Remove from assigned workers list. If it wasn't there, then don't continue with this function
        if (!assignedAgents.Remove(agent)) return;

        agent.GetTask().DecrementAssignment();
        agent.SetTask(null);
        unassignedAgents.Add(agent);
    }

    /// <summary>
    /// Register a new <c>TaskRule</c> - this is assumed to not be a recursive process, i.e. rules cannot create other rules.
    /// Otherwise, this operation may lead to concurrent modification exceptions.
    /// </summary>
    public void RegisterRule(TaskRule rule) {
        if (taskRules.Contains(rule)) return;

        taskRules.Add(rule);
    }

    /// <summary>
    /// Deregister a new <c>TaskRule</c>. We assume that the instance is NOT calling this itself, otherwise it could
    /// create a concurrent modification exception.
    /// </summary>
    public void DeregisterRule(TaskRule rule) {
        taskRules.Remove(rule);
        rule.OnDestruction();
    }

    public void CreateTask(Task task) {
        if (task == null) return;

        // Add the task to the correct list, depending on its type
        pendingAdditionTasks.Add(task);
    }

    public void MarkComplete(Task task) {
        pendingCompletionTasks.Add(task);
    }
}