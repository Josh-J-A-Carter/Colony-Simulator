using UnityEngine;

public interface IEntity {
    
    // public virtual void OnCreation() {}

    // public virtual void OnDestruction() {}

    public GameObject GetGameObject();

    public void SetOutline();

    public void ResetOutline();

}
