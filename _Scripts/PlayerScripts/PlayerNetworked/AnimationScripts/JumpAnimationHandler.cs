using UnityEngine;
using Unity.Netcode;

public class JumpAnimationHandler : MonoBehaviour, IAnimationStateHandler
{
    private NetworkObject parentNetworkObject;

    private void Awake()
    {
        parentNetworkObject = GetComponentInParent<NetworkObject>();
    }

    public void UpdateState(Animator animator)
    {
        if (parentNetworkObject == null || !parentNetworkObject.IsOwner || animator == null)
            return;

        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("Jump");
        }
    }
}
