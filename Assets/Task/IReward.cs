using System.Collections.Generic;
using System.Linq;

public interface IReward {

    public List<(Item, uint)> GetRewardItems();
}
