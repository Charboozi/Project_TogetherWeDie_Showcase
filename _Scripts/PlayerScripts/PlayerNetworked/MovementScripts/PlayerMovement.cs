using UnityEngine;
using Unity.Netcode;
using System.Collections;


[RequireComponent(typeof(CharacterController))]
public class NetworkedCharacterMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;
    public float jumpForce = 2.0f;
    public float gravity = 9.8f;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    public bool IsGrounded => isGrounded;

    private float bonusSpeed = 0f;
    private float slowMultiplier = 1f;
    private Coroutine slowCoroutine;
    private Transform currentLift;
    private Vector3 lastLiftPosition;

    public Vector3 MovementVelocity { get; private set; }

    private EntityHealth entityHealth;

    // 👇 New Input System
    private PlayerControls input;
    private Vector2 moveInput;
    private bool jumpQueued;

    public float CurrentMoveSpeed => (baseMoveSpeed + bonusSpeed) * slowMultiplier;

    private void Awake()
    {
        input = new PlayerControls();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Jump.performed += _ => jumpQueued = true;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        entityHealth = GetComponent<EntityHealth>();
        velocity = Vector3.zero;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (currentLift != null)
        {
            Vector3 liftDelta = currentLift.position - lastLiftPosition;
            controller.Move(liftDelta);
            lastLiftPosition = currentLift.position;
        }

        if (entityHealth != null && entityHealth.isDowned.Value)
            return;

        CheckGrounded();
        ProcessMovement();
        ProcessJump();
    }

    private void ProcessMovement()
    {
        Vector3 moveDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        MovementVelocity = moveDir * CurrentMoveSpeed;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            controller.Move(MovementVelocity * Time.deltaTime);
        }
    }

    private void ProcessJump()
    {
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (isGrounded && jumpQueued)
        {
            velocity.y = Mathf.Sqrt(jumpForce * 2f * gravity);
            jumpQueued = false;
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckGrounded()
    {
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2f);
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer);
    }

    public void ApplyTemporarySlow(float slowFactor, float duration)
    {
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowDownCoroutine(slowFactor, duration));
    }

    private IEnumerator SlowDownCoroutine(float slowFactor, float duration)
    {
        slowMultiplier = 1f - slowFactor;
        yield return new WaitForSeconds(duration);
        slowMultiplier = 1f;
    }

    public void AddBonusSpeed(float amount)
    {
        bonusSpeed += amount;
    }

    public void RemoveBonusSpeed(float amount)
    {
        bonusSpeed -= amount;
        if (bonusSpeed < 0f) bonusSpeed = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lift"))
        {
            currentLift = other.transform;
            lastLiftPosition = currentLift.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Lift"))
        {
            currentLift = null;
        }
    }
}
