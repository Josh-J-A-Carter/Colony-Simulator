using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Tool {
    
    public virtual void OnEquip() {}

    public virtual void OnDequip() {}

    public abstract void Run(HoverData hoverData);

}
