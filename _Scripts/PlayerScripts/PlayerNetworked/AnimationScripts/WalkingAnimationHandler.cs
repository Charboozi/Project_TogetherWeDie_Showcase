using UnityEngine;
using Unity.Netcode;

public class WalkAnimationHandler : MonoBehaviour, IAnimationStateHandler
{
    private CharacterController characterController;
    private NetworkObject parentNetworkObject;

    private void Awake()
    {
        characterController = GetComponentInParent<CharacterController>();
        parentNetworkObject = GetComponentInParent<NetworkObject>();
    }

    public void UpdateState(Animator animator)
    {
        if (parentNetworkObject == null || !parentNetworkObject.IsOwner || animator == null || characterController == null)
            return;

        bool isGrounded = characterController.isGrounded;
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        animator.SetBool("isWalking", isGrounded && isMoving);
    }
}
