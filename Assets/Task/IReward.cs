using System.Collections.Generic;
using System.Collections.ObjectModel;

public interface IReward {

    public List<(Item, uint)> CollectRewardItems();

    public List<Item> GetAvailableRewardItems();
}
