using UnityEngine;

public class ProceduralArmsAnimation : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobSpeed = 5f;
    public float bobAmount = 0.05f;

    [Header("Sway Settings")]
    public float swayAmount = 0.02f;

    [Header("Idle (Breathing) Settings")]
    public float idleSpeed = 1f;
    public float idleAmount = 0.01f;
    public float idleSmoothness = 2f;

    [Header("Camera-Based Position Sway")]
    public float positionSwayAmount = 0.02f;

    [Header("Jump Reaction Settings")]
    public float jumpReactionOffset = 0.1f;
    public float jumpLandSmoothness = 5f;

    private Vector3 originalPosition;
    private float bobTimer = 0f;
    private float idleTimer = 0f;
    private float currentIdleOffset = 0f;
    private Vector3 lastCameraPosition;

    private Vector2 lastMoveInput = Vector2.zero;
    private Vector2 mouseLookInput = Vector2.zero;

    private Vector3 bobOffset = Vector3.zero;
    private Vector3 swayOffset = Vector3.zero;
    private Vector3 idleOffset = Vector3.zero;
    private Vector3 posSwayOffset = Vector3.zero;
    private Vector3 jumpOffsetEffect = Vector3.zero;

    private Camera playerCamera;

    private void OnEnable()
    {
        PlayerInput.OnMoveInput += HandleMovementInput;
        PlayerInput.OnMouseLook += HandleMouseLook;
    }

    private void OnDisable()
    {
        PlayerInput.OnMoveInput -= HandleMovementInput;
        PlayerInput.OnMouseLook -= HandleMouseLook;
    }

    private void HandleMovementInput(Vector2 moveInput)
    {
        lastMoveInput = moveInput;
    }

    private void HandleMouseLook(Vector2 mouseDelta)
    {
        mouseLookInput = mouseDelta;
    }


    void Start()
    {
        originalPosition = transform.localPosition;
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No Main Camera found! Please tag your camera as 'MainCamera'.");
            return;
        }

        lastCameraPosition = playerCamera.transform.position;
    }

    void Update()
    {
        // ===== Bobbing Effect (based on movement input) =====
        float inputMagnitude = Mathf.Abs(lastMoveInput.x) + Mathf.Abs(lastMoveInput.y);
        if (inputMagnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobX = Mathf.Sin(bobTimer) * bobAmount;
            float bobY = Mathf.Cos(bobTimer * 2f) * (bobAmount * 0.5f);
            bobOffset = new Vector3(bobX, bobY, 0);
        }
        else
        {
            bobTimer = 0f;
            bobOffset = Vector3.zero;
        }

        // ===== Sway Effect (based on mouse movement) =====
        float swayX = mouseLookInput.x * swayAmount;
        float swayY = mouseLookInput.y * swayAmount;
        swayOffset = new Vector3(-swayX, -swayY, 0);

        // ===== Idle Breathing Effect =====
        idleTimer += Time.deltaTime * idleSpeed;
        float targetIdle = Mathf.Sin(idleTimer) * idleAmount;
        currentIdleOffset = Mathf.Lerp(currentIdleOffset, targetIdle, Time.deltaTime * idleSmoothness);
        idleOffset = new Vector3(0, currentIdleOffset, 0);

        // ===== Camera-Based Position Sway =====
        Vector3 camDelta = playerCamera.transform.position - lastCameraPosition;
        posSwayOffset = new Vector3(-camDelta.x, -camDelta.y, 0) * positionSwayAmount;

        // ===== Jump Reaction Effect =====
        if (playerCamera.transform.position.y < lastCameraPosition.y - 0.05f)
        {
            jumpOffsetEffect = Vector3.Lerp(jumpOffsetEffect, new Vector3(0, -jumpReactionOffset, 0), Time.deltaTime * jumpLandSmoothness);
        }
        else
        {
            jumpOffsetEffect = Vector3.Lerp(jumpOffsetEffect, Vector3.zero, Time.deltaTime * jumpLandSmoothness);
        }

        // ===== Combine all offsets =====
        Vector3 targetOffset = originalPosition + bobOffset + swayOffset + idleOffset + posSwayOffset + jumpOffsetEffect;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetOffset, Time.deltaTime * 10f);

        lastCameraPosition = playerCamera.transform.position;
    }
}
