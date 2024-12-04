using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour {

    public static TaskManager Instance { get; private set; }

    List<Task> pendingCompleteTasks;

    List<Task> workerTasks;
    List<WorkerBehaviour> assignedWorkers, unassignedWorkers;

    List<Task> queenTasks;
    List<QueenBehaviour> assignedQueens, unassignedQueens;

    void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        pendingCompleteTasks = new List<Task>();

        workerTasks = new List<Task>();
        assignedWorkers = new List<WorkerBehaviour>();
        unassignedWorkers = new List<WorkerBehaviour>();

        queenTasks = new List<Task>();
        assignedQueens = new List<QueenBehaviour>();
        unassignedQueens = new List<QueenBehaviour>();
    }

    void Update() {

        // Deal with the queue of tasks that have been marked as complete
        ClearPendingTasks();


        // 
        // To do: Deal with urgent tasks or something
        // 


        // Deal with agents that currently are not assigned a task
        OccupyUnassignedAgents();
    }

    void OccupyUnassignedAgents() {        
        for (int i = 0 ; i < unassignedWorkers.Count ; i += 1) {
            WorkerTask task = (WorkerTask) GetMostUrgent(workerTasks);
            if (task == null) break;
            WorkerBehaviour worker = unassignedWorkers[i];

            if (worker.OfferTask(task)) {
                assignedWorkers.Add(worker);
                unassignedWorkers.RemoveAt(i);
                task.IncrementAssignment();
                i -= 1;
                continue;
            }
        }

        for (int i = 0 ; i < unassignedQueens.Count ; i += 1) {
            QueenTask task = (QueenTask) GetMostUrgent(queenTasks);
            if (task == null) break;
            QueenBehaviour queen = unassignedQueens[i];

            if (queen.OfferTask(task)) {
                assignedQueens.Add(queen);
                unassignedQueens.RemoveAt(i);
                task.IncrementAssignment();
                i -= 1;
                continue;
            }
        }
    }

    Task GetMostUrgent(List<Task> taskList) {
        if (taskList.Count == 0) return null;

        Task mostUrgent = taskList[0];

        foreach (Task task in taskList) {
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

    void ClearPendingTasks() {
        foreach (Task task in pendingCompleteTasks) {
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

                continue;
            }

            if (task is QueenTask queenTask) {
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

                continue;
            }
        }

        pendingCompleteTasks.Clear();
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
        if (task is WorkerTask workerTask) workerTasks.Add(workerTask);
        else if (task is QueenTask queenTask) queenTasks.Add(queenTask);

        task.OnCreation();
    }

    public void MarkComplete(Task task) {
        pendingCompleteTasks.Add(task);
    }
}

