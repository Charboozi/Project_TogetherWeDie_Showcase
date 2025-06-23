using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(Interactable))]
public class SlidingDoor : NetworkBehaviour, IInteractableAction
{
    [Header("Door Parts")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Slide Offsets")]
    [SerializeField] private Vector3 leftOffset = new Vector3(-2f, 0f, 0f);
    [SerializeField] private Vector3 rightOffset = new Vector3(2f, 0f, 0f);

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Lock Settings")]
    [SerializeField] private bool startLocked = true;

    [Header("Status Light")]
    [SerializeField] private Renderer statusLightRenderer;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedClosedColor = Color.yellow;
    [SerializeField] private Color openColor = Color.green;

    private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(
    true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool isSliding = false;

    public bool IsLocked => isLocked.Value;

    private Vector3 leftClosedPos, rightClosedPos;
    private Vector3 leftOpenPos, rightOpenPos;

    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        leftOpenPos = leftClosedPos + leftOffset;
        rightOpenPos = rightClosedPos + rightOffset;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            bool isPvP = GameModeManager.Instance != null && GameModeManager.Instance.IsPvPMode;
            isLocked.Value = isPvP ? false : startLocked;
            isOpen.Value = !isLocked.Value;
        }

        isOpen.OnValueChanged += (_, _) => AnimateDoor(isOpen.Value);
        isLocked.OnValueChanged += (_, _) => UpdateStatusLight();

        AnimateDoor(isOpen.Value);
        UpdateStatusLight(); // ✅ Add this to ensure client sees correct color immediately
    }

    public void DoAction()
    {
        if (!IsServer || isSliding) return;

        if (isLocked.Value)
        {
            isLocked.Value = false;
            Debug.Log("🔓 Door unlocked via Interactable keycard use.");
        }
        else
        {
            Debug.Log("🔁 Door toggled via Interactable keycard use.");
        }

        isOpen.Value = !isOpen.Value;
        UpdateStatusLight();
    }

    private void AnimateDoor(bool opening)
    {
        StartCoroutine(SlideDoors(opening));
        UpdateStatusLight();
    }

    private IEnumerator SlideDoors(bool opening)
    {
        isSliding = true;

        Vector3 leftStart = leftDoor.localPosition;
        Vector3 leftTarget = opening ? leftOpenPos : leftClosedPos;

        Vector3 rightStart = rightDoor.localPosition;
        Vector3 rightTarget = opening ? rightOpenPos : rightClosedPos;

        float timer = 0f;

        while (timer < slideDuration)
        {
            float t = slideCurve.Evaluate(timer / slideDuration);
            leftDoor.localPosition = Vector3.Lerp(leftStart, leftTarget, t);
            rightDoor.localPosition = Vector3.Lerp(rightStart, rightTarget, t);

            timer += Time.deltaTime;
            yield return null;
        }

        leftDoor.localPosition = leftTarget;
        rightDoor.localPosition = rightTarget;
        isSliding = false;
    }

    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");
    private MaterialPropertyBlock mpb;

    private void UpdateStatusLight()
    {
        if (statusLightRenderer == null) return;

        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        Color color = isLocked.Value ? lockedColor : (isOpen.Value ? openColor : unlockedClosedColor);

        mpb.SetColor(ColorID, color);
        mpb.SetColor(EmissionID, color);

        statusLightRenderer.SetPropertyBlock(mpb);
    }

    public void Open()
    {
        if (!IsServer || isSliding || isOpen.Value) return;
        isOpen.Value = true;
    }

    public void Close()
    {
        if (!IsServer || isSliding || !isOpen.Value) return;
        isOpen.Value = false;
    }
}
