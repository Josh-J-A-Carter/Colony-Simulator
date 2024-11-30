using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tool : MonoBehaviour {

    protected ToolManager parent;

    public void SetUp(ToolManager parent) {
        this.parent = parent;
    }
    
    public virtual void OnEquip() {}

    public virtual void OnDequip() {}

    public abstract void Run(HoverData hoverData);

}
