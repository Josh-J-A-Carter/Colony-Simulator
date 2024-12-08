public abstract class WorkerTask : Task {
    public abstract WorkerTaskType GetCategory();
}

public enum WorkerTaskType {
    House,
    Forage,
    Nurse
}
