
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

    public override String GetName() {
        return "Attack";
    }

    public override String GetDescription() {
        return "Attacking any living organisms that threaten the security of the hive";
    }

    public override InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new InfoBranch(String.Empty);
        
        InfoLeaf nameProperty = new InfoLeaf("Type", "Attack");
        root.AddChild(nameProperty);

        if (target is IInformative info) {
            InfoLeaf progressProperty = new InfoLeaf("Target", info.GetName());
            root.AddChild(progressProperty);
        }

        return root;
    }
}
