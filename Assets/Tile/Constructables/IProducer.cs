using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public interface IProducer {

    /// <summary>
    /// What item types are currently available?
    /// </summary>
    public List<Item> AvailableProductionItemTypes(Dictionary<String, object> instance);

    /// <summary>
    /// What items can this IProducer create?
    /// </summary>
    public ReadOnlyCollection<Item> ProductionItemTypes();
    
    /// <summary>
    /// Collect all items that are ready to be harvested
    /// </summary>
    public List<(Item, uint)> CollectAll(Dictionary<String, object> instance);
}
