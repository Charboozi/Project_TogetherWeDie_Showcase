using UnityEngine;
using Unity.Netcode;

public class PlayerDowned : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private EntityHealth entityHealth;

    private void Awake()
    {
        entityHealth = GetComponent<EntityHealth>();
    }

    private void Update()
    {
        if (entityHealth != null)
        {
            animator.SetBool("Downed", entityHealth.isDowned.Value);
        }
    }
}
