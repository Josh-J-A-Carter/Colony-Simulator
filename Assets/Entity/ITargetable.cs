using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable {

    public int Friendliness();

    public Vector2 GetPosition();
    
    public void Damage(uint amount);

}
