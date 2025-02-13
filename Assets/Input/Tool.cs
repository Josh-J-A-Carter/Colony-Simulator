using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tool : MonoBehaviour {

    protected InputManager parent;

    public void SetUp(InputManager parent) {
        this.parent = parent;
    }
    
    public virtual void OnEquip() {}

    public virtual void OnDequip() {}

    public virtual void Run(HoverData hoverData) {}

    public virtual void FixedRun() {}

}
