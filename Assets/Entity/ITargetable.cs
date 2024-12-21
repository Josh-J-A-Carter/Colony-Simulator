using UnityEngine;

public interface ITargetable {

    public bool IsDead { get; }

    public int Friendliness();

    public Vector2 GetPosition();
    
    public void Damage(uint amount);
}
