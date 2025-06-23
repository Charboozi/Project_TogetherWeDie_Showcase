using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private List<IAnimationStateHandler> animationHandlers = new();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animationHandlers.AddRange(GetComponents<IAnimationStateHandler>());
    }

    private void Update()
    {
        foreach (var handler in animationHandlers)
        {
            handler.UpdateState(animator);
        }
    }
}
