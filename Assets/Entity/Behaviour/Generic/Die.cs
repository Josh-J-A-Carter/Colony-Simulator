using UnityEngine;

public class Die : State {
    
    [SerializeField]
    AnimationClip anim;

    public override void OnEntry() {
        animator.Play(anim.name);
    }
}
