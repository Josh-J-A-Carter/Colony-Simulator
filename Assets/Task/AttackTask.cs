
public class AttackTask : Task {

    ITargetable target;

    public AttackTask(ITargetable target, TaskPriority priority) {
        this.target = target;
        this.priority = priority;
    }

    public override bool EarlyCompletion() {
        return target == null || target.IsDead();
    }

    public override string GetDescription() {
        throw new System.NotImplementedException();
    }

    public override InfoBranch GetInfoTree(object obj = null) {
        throw new System.NotImplementedException();
    }

    public override string GetName() {
        throw new System.NotImplementedException();
    }

    public ITargetable GetTarget() {
        return target;
    }

    public override bool IsWorkerTask() {
        return true;
    }

    public override bool IsQueenTask() {
        return true;
    }
}
