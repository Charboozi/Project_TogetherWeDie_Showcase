using UnityEngine;
using Unity.Netcode;

public class FlammablePoster : NetworkBehaviour
{
    [Header("Poster States")]
    [SerializeField] private GameObject intactPoster;
    [SerializeField] private GameObject destroyedPoster;

    [Header("Hit Settings")]
    [SerializeField] private float damageThreshold = 1.5f;

    [Header("Linked Button Movement")]
    [SerializeField] private Transform linkedButton;
    [SerializeField] private Transform targetPoint;

    private float damageAccumulated = 0f;

    private NetworkVariable<bool> isDestroyed = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        SetPosterState(true);
    }

    public override void OnNetworkSpawn()
    {
        isDestroyed.OnValueChanged += (_, newValue) =>
        {
            SetPosterState(!newValue);
        };
        SetPosterState(!isDestroyed.Value);
    }

    // ✅ Called from any local script (even non-networked)
    public void BurnRequest(float amount)
    {
        if (IsServer)
        {
            ApplyBurn(amount);
        }
        else
        {
            RequestBurnServerRpc(amount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBurnServerRpc(float amount)
    {
        ApplyBurn(amount);
    }

    private void ApplyBurn(float amount)
    {
        if (isDestroyed.Value) return;

        damageAccumulated += amount;

        if (damageAccumulated >= damageThreshold)
        {
            isDestroyed.Value = true;
            MoveButtonToTarget();
            Debug.Log($"🔥 Poster {name} destroyed!");
        }
    }

    private void SetPosterState(bool intact)
    {
        intactPoster?.SetActive(intact);
        destroyedPoster?.SetActive(!intact);
    }

    private void MoveButtonToTarget()
    {
        if (linkedButton == null || targetPoint == null) return;

        linkedButton.position = targetPoint.position;
        Debug.Log($"🟦 Button {linkedButton.name} moved to {targetPoint.position}");
    }
}
