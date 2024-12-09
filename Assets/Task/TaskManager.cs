using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour {

    public static TaskManager Instance { get; private set; }

    LocativeTaskStore locativeTaskStore;

    List<Task> pendingCompletionTasks;
    List<Task> pendingAdditionTasks;

    List<Task> tasks;
    List<TaskAgent> assignedAgents, unassignedAgents;

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        locativeTaskStore = new();

        pendingCompletionTasks = new();
        pendingAdditionTasks = new();

        tasks = new();
        assignedAgents = new();
        unassignedAgents = new();
    }

    public void FixedUpdate() {

        // Deal with the queue of tasks that have been marked as complete
        ClearPendingCompletion();

        // Deal with the queue of tasks that are waiting to be added
        ClearPendingAddition();

        // 
        // To do: Deal with urgent tasks or something
        // 


        // Deal with agents that currently are not assigned a task
        OccupyUnassignedAgents();
    }

    void OccupyUnassignedAgents() {
        for (int i = 0 ; i < unassignedAgents.Count ; i += 1) {
            TaskAgent agent = unassignedAgents[i];
            SortTasksByUrgency();

            // String str = "";
            // foreach (Task task in tasks) str = $"{str}   ({task.priority}, {task.assignment}, {task.creationTime})";
            // Debug.Log(str);

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

    // Task GetMostUrgent() {
    //     if (tasks.Count == 0) return null;

    //     Task mostUrgent = tasks[0];

    //     foreach (Task task in tasks) {
    //         if (task.priority > mostUrgent.priority) continue;
    //         else if (task.priority < mostUrgent.priority) {
    //             mostUrgent = task;
    //             continue;
    //         }

    //         if (task.assignment > mostUrgent.assignment) continue;
    //         else if (task.assignment < mostUrgent.assignment) {
    //             mostUrgent = task;
    //             continue;
    //         }

    //         if (task.creationTime > mostUrgent.creationTime) continue;
    //         else if (task.creationTime < mostUrgent.creationTime) {
    //             mostUrgent = task;
    //             continue;
    //         }
    //     }

    //     return mostUrgent;
    // }

    void ClearPendingCompletion() {
        foreach (Task task in pendingCompletionTasks) {
            
            // Tell the task that it is complete
            task.OnCompletion();

            // Remove the task from the list
            tasks.Remove(task);

            // Reset all those agents whose task is set to this one
            for (int i = 0 ; i < assignedAgents.Count ; i += 1) {
                if (assignedAgents[i].GetTask() != task) continue;

                assignedAgents[i].SetTask(null);
                unassignedAgents.Add(assignedAgents[i]);
                assignedAgents.RemoveAt(i);
                i -= 1;
            }

            // Unset locative task store!
            if (task is Locative locativeTask) locativeTaskStore.UnsetTask(locativeTask);
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
                if (task is Locative locativeTask) locativeTaskStore.SetTask(locativeTask);
            }
        }
        
        // Need to add them to a confirmation queue; two pending tasks might be added at the same time and overlap
        foreach (Task task in toConfirm) task.Confirm();

        pendingAdditionTasks.Clear();
    }

    bool ResolveConflicts(Task task) {
        if (task.MustAbort()) return false;

        if (task is Locative locative) {
            List<Task> conflicts = locativeTaskStore.GetConflictingTasks(locative);

            // Look for a reason to cancel the new task (i.e. conflicts with another pending task)
            foreach (Task conflict in conflicts) if (conflict.IsPendingConfirmation()) return false;

            // No other pending tasks conflict with this one; therefore, all the other
            // conflicting tasks must be older than this one, and can thus be destroyed
            foreach (Task conflict in conflicts) CancelTask(conflict);
        }
 
        return true;
    }

    void CancelTask(Task task) {
        
        // Tell the task that it has been cancelled
        task.OnCancellation();

        // Remove the task from the list
        tasks.Remove(task);

        // If applicable, de-allocate the task's resources (i.e. items needed for its completion)
        if (task is Consumer consumer) Deallocate(consumer);

        // Reset all those agents whose task is set to this one
        for (int i = 0 ; i < assignedAgents.Count ; i += 1) {
            if (assignedAgents[i].GetTask() != task) continue;

            assignedAgents[i].OnTaskCancellation();
            unassignedAgents.Add(assignedAgents[i]);
            assignedAgents.RemoveAt(i);
            i -= 1;
        }
        
        // Unset locative task store!
        if (task is Locative locativeTask) locativeTaskStore.UnsetTask(locativeTask);
    }

    public void Allocate(Consumer consumer, InventoryManager inventory) {
        if (consumer == null || !inventory) return;

        if (consumer.HasAllocation()) return;

        foreach ((Item item, uint quantity) in consumer.GetRequiredResources()) if (!inventory.Has(item, quantity)) return;

        consumer.Allocate(inventory);
        
        foreach ((Item item, uint quantity) in consumer.GetRequiredResources()) inventory.Take(item, quantity);
    }

    void Deallocate(Consumer consumer) {
        if (!consumer.HasAllocation()) return;

        InventoryManager inventory = consumer.GetAllocator();

        if (inventory) {
            inventory.Give(consumer.GetRequiredResources().ToList());
            return;
        }

        Vector2Int pos = consumer.GetDefaultDeallocationPosition();
        foreach ((Item item, uint quantity) in consumer.GetRequiredResources()) {
            EntityManager.Instance.InstantiateItemEntity(pos, item, quantity);
        }
    }

    public void RegisterAgent(TaskAgent agent) {
        unassignedAgents.Add(agent);
    }

    public void DeregisterAgent(TaskAgent agent) {
        if (unassignedAgents.Remove(agent)) return;

        agent.GetTask().DecrementAssignment();
        agent.SetTask(null);
        assignedAgents.Remove(agent);
    }

    public void UnassignAgent(TaskAgent agent) {
        // Remove from assigned workers list. If it wasn't there, then don't continue with this function
        if (!assignedAgents.Remove(agent)) return;

        agent.GetTask().DecrementAssignment();
        agent.SetTask(null);
        unassignedAgents.Add(agent);
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