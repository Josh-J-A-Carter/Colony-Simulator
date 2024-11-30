public abstract class WorkerTask : Task {
    public WorkerTaskType category { get; protected set; }
}

public enum WorkerTaskType {
    Hive,
    Forage,
    Nurse
}
