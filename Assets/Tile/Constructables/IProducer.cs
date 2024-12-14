using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public interface IProducer {

    /// <summary>
    /// Is this instance of IProducer ready to have <c>item</c> harvested?
    /// </summary>
    public bool IsReady(Dictionary<String, object> instance, Item item);

    /// <summary>
    /// Does this instance of IProducer have anything ready to harvest?
    /// </summary>
    public bool IsReady(Dictionary<String, object> instance);

    /// <summary>
    /// What items can this IProducer create?
    /// </summary>
    public ReadOnlyCollection<Item> ProductionItemTypes();
    
    /// <summary>
    /// Collect all items that are ready to be harvested
    /// </summary>
    public List<(Item, uint)> CollectAll(Dictionary<String, object> instance);
}
