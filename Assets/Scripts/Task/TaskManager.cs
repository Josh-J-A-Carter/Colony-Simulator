using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour {

    public static TaskManager Instance { get; private set; }

    List<Task> pendingCompleteTasks;
    List<WorkerTask> workerTasks;
    List<WorkerBehaviour> assignedWorkers;
    List<WorkerBehaviour> unassignedWorkers;

    [SerializeField]
    Constructable comb;


    void Awake() {
        if (Instance == null) Instance = this;
        else {
            Destroy(this);
            return;
        }

        pendingCompleteTasks = new List<Task>();
        workerTasks = new List<WorkerTask>();

        assignedWorkers = new List<WorkerBehaviour>();
        unassignedWorkers = new List<WorkerBehaviour>();
    }

    void Start() {
        CreateTask(new BuildTask(TaskPriority.Normal, new Vector2Int(-6, -5), comb));
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
        if (workerTasks.Count == 0) return;

        for (int i = 0 ; i < unassignedWorkers.Count ; i += 1) {
            WorkerTask task = GetMostUrgent(workerTasks);
            WorkerBehaviour worker = unassignedWorkers[i];

            if (worker.OfferTask(task)) {
                assignedWorkers.Add(worker);
                unassignedWorkers.RemoveAt(i);
                task.IncrementAssignment();
                i -= 1;
                continue;
            }
        }
    }

    WorkerTask GetMostUrgent(List<WorkerTask> taskList) {
        if (taskList.Count == 0) return null;

        WorkerTask mostUrgent = taskList[0];

        foreach (WorkerTask task in taskList) {
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
        }

        pendingCompleteTasks.Clear();
    }

    public void RegisterAgent(TaskAgent agent) {
        if (agent is WorkerBehaviour worker) {
            unassignedWorkers.Add(worker);
            return;
        }
    }

    public void DeregisterAgent(TaskAgent agent) {
        if (agent is WorkerBehaviour worker) {
            if (unassignedWorkers.Remove(worker)) return;

            worker.GetTask().DecrementAssignment();
            worker.SetTask(null);
            assignedWorkers.Remove(worker);

            return;
        }
    }

    public void UnassignAgent(TaskAgent agent) {
        if (agent is WorkerBehaviour worker) {
            // Remove from assigned workers list. If it wasn't there, then don't continue with this function
            if (!assignedWorkers.Remove(worker)) return;

            worker.GetTask().DecrementAssignment();
            worker.SetTask(null);
            unassignedWorkers.Add(worker);
        }
    }

    public void CreateTask(Task task) {
        if (task is WorkerTask workerTask) {
            workerTasks.Add(workerTask);
            workerTask.OnCreation();
            return;
        }
    }

    public void MarkComplete(Task task) {
        pendingCompleteTasks.Add(task);
    }
}

