using System.Collections.Generic;
using System.Linq;

public interface IReward {

    public List<(Item, uint)> GetRewardItems();

    public virtual void GiveReward(InventoryManager inventory) {
        inventory.Give(GetRewardItems().ToList());
    }

}
