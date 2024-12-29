
using System;

public class AttackTask : Task {

    ITargetable target;

    public AttackTask(ITargetable target, TaskPriority priority) {
        this.target = target;
        this.priority = priority;
    }

    public override bool EarlyCompletion() {
        return target == null || target.IsDead();
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

    protected override String GetProgress() {
        return "Incomplete";
    }


    protected override String GetTaskCategory() {
        return "Soldier duties";
    }

    protected override String GetTaskType() {
        return "Attacking enemies";
    }
}
