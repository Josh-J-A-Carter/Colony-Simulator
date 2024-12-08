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

    List<WorkerTask> workerTasks;
    List<WorkerBehaviour> assignedWorkers, unassignedWorkers;

    List<QueenTask> queenTasks;
    List<QueenBehaviour> assignedQueens, unassignedQueens;

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        locativeTaskStore = new LocativeTaskStore();

        pendingCompletionTasks = new List<Task>();
        pendingAdditionTasks = new List<Task>();

        workerTasks = new List<WorkerTask>();
        assignedWorkers = new List<WorkerBehaviour>();
        unassignedWorkers = new List<WorkerBehaviour>();

        queenTasks = new List<QueenTask>();
        assignedQueens = new List<QueenBehaviour>();
        unassignedQueens = new List<QueenBehaviour>();
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
        for (int i = 0 ; i < unassignedWorkers.Count ; i += 1) {
            WorkerTask task = GetMostUrgent(workerTasks);
            if (task == null) break;
            WorkerBehaviour worker = unassignedWorkers[i];

            if (worker.OfferTask(task)) {
                assignedWorkers.Add(worker);
                unassignedWorkers.RemoveAt(i);
                task.IncrementAssignment();
                i -= 1;
            }
        }

        for (int i = 0 ; i < unassignedQueens.Count ; i += 1) {
            QueenTask task = GetMostUrgent(queenTasks);
            if (task == null) break;
            QueenBehaviour queen = unassignedQueens[i];

            if (queen.OfferTask(task)) {
                assignedQueens.Add(queen);
                unassignedQueens.RemoveAt(i);
                task.IncrementAssignment();
                i -= 1;
            }
        }
    }

    T GetMostUrgent<T>(List<T> taskList) where T: Task {
        if (taskList.Count == 0) return null;

        T mostUrgent = taskList[0];

        foreach (T task in taskList) {
            if (task.priority > mostUrgent.priority) continue;
            else if (task.priority < mostUrgent.priority) {
                mostUrgent = task;
                continue;
            }

            if (task.assignment > mostUrgent.assignment) continue;
            else if (task.assignment < mostUrgent.assignment) {
                mostUrgent = task;
                continue;
            }

            if (task.creationTime > mostUrgent.creationTime) continue;
            else if (task.creationTime < mostUrgent.creationTime) {
                mostUrgent = task;
                continue;
            }
        }

        return mostUrgent;
    }

    void ClearPendingCompletion() {
        foreach (Task task in pendingCompletionTasks) {
            if (task is WorkerTask workerTask) {
                // Tell the task that it is complete
                workerTask.OnCompletion();

                // Remove the task from the list
                workerTasks.Remove(workerTask);

                // Reset all those agents whose task is set to this one
                for (int i = 0 ; i < assignedWorkers.Count ; i += 1) {
                    if (assignedWorkers[i].GetTask() != workerTask) continue;

                    assignedWorkers[i].SetTask(null);
                    unassignedWorkers.Add(assignedWorkers[i]);
                    assignedWorkers.RemoveAt(i);
                    i -= 1;
                }
            }

            else if (task is QueenTask queenTask) {
                // Tell the task that it is complete
                queenTask.OnCompletion();

                // Remove the task from the list
                queenTasks.Remove(queenTask);

                // Reset all those agents whose task is set to this one
                for (int i = 0 ; i < assignedQueens.Count ; i += 1) {
                    if (assignedQueens[i].GetTask() != queenTask) continue;

                    assignedQueens[i].SetTask(null);
                    unassignedQueens.Add(assignedQueens[i]);
                    assignedQueens.RemoveAt(i);
                    i -= 1;
                }
            }

            else throw new System.Exception("Task type not implemented in ClearPendingCompletion function");

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
                // Add the task to the correct list, depending on its type
                if (task is WorkerTask workerTask) workerTasks.Add(workerTask);
                else if (task is QueenTask queenTask) queenTasks.Add(queenTask);
                else throw new System.Exception("Task pending addition by TaskManager; task-type unrecognised.");

                toConfirm.Add(task);
                // Pre-emptively add to the locative task store;
                // can't quite set it to be confirmed yet, as this will remove the pending status,
                // allowing other pending tasks (added at the same time) to override this,
                // when in reality they have the same age
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
        if (task is WorkerTask workerTask) {
            // Tell the task that it has been cancelled
            workerTask.OnCancellation();

            // Remove the task from the list
            workerTasks.Remove(workerTask);

            // If applicable, de-allocate the task's resources (i.e. items needed for its completion)
            if (task is Consumer consumer) Deallocate(consumer);

            // Reset all those agents whose task is set to this one
            for (int i = 0 ; i < assignedWorkers.Count ; i += 1) {
                if (assignedWorkers[i].GetTask() != workerTask) continue;

                assignedWorkers[i].OnTaskCancellation();
                unassignedWorkers.Add(assignedWorkers[i]);
                assignedWorkers.RemoveAt(i);
                i -= 1;
            }
        }

        else if (task is QueenTask queenTask) {
            // Tell the task that it is complete
            queenTask.OnCancellation();

            // Remove the task from the list
            queenTasks.Remove(queenTask);

            // If applicable, de-allocate the task's resources (i.e. items needed for its completion)
            if (task is Consumer consumer) Deallocate(consumer);

            // Reset all those agents whose task is set to this one
            for (int i = 0 ; i < assignedQueens.Count ; i += 1) {
                if (assignedQueens[i].GetTask() != queenTask) continue;

                assignedQueens[i].CancelTask();
                unassignedQueens.Add(assignedQueens[i]);
                assignedQueens.RemoveAt(i);
                i -= 1;
            }
        }

        else throw new System.Exception("Task type not implemented in CancelTask function");

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
        if (agent is WorkerBehaviour worker) unassignedWorkers.Add(worker);
        else if (agent is QueenBehaviour queen) unassignedQueens.Add(queen);
    }

    public void DeregisterAgent(TaskAgent agent) {
        if (agent is WorkerBehaviour worker) {
            if (unassignedWorkers.Remove(worker)) return;

            worker.GetTask().DecrementAssignment();
            worker.SetTask(null);
            assignedWorkers.Remove(worker);
        } else if (agent is QueenBehaviour queen) {
            if (unassignedQueens.Remove(queen)) return;

            queen.GetTask().DecrementAssignment();
            queen.SetTask(null);
            assignedQueens.Remove(queen);
        }
    }

    public void UnassignAgent(TaskAgent agent) {
        if (agent is WorkerBehaviour worker) {
            // Remove from assigned workers list. If it wasn't there, then don't continue with this function
            if (!assignedWorkers.Remove(worker)) return;

            worker.GetTask().DecrementAssignment();
            worker.SetTask(null);
            unassignedWorkers.Add(worker);
        } else if (agent is QueenBehaviour queen) {
            // Remove from assigned workers list. If it wasn't there, then don't continue with this function
            if (!assignedQueens.Remove(queen)) return;

            queen.GetTask().DecrementAssignment();
            queen.SetTask(null);
            unassignedQueens.Add(queen);
        }
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