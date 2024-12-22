using UnityEngine;

public interface ITargetable {

    public bool IsDead();

    public int Friendliness();

    public Vector2 GetPosition();
    
    public void Damage(uint amount, ITargetable attacker = null);
}
