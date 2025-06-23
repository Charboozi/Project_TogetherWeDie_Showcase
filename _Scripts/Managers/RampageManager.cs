using System;
using UnityEngine;
using Unity.Netcode;

public class RampageManager : NetworkBehaviour
{
    public static RampageManager Instance { get; private set; }

    [Header("Rampage Settings")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private Color rampageEyeColor = Color.red;

    private bool rampageActive = false;

    public event Action<float, Color> OnRampageStart;
    public event Action OnRampageEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartRampage()
    {
        if (!IsServer)
        {
            Debug.LogWarning("RampageManager: only the server can start rampage.");
            return;
        }

        Debug.Log("🚨 Rampage started.");
        StartRampageClientRpc(speedMultiplier, rampageEyeColor);
        StartRampageLocally(speedMultiplier, rampageEyeColor);
        GameFeedManager.Instance?.PostFeedMessage("Rampage mode activated!!");
    }

    [ClientRpc]
    private void StartRampageClientRpc(float mult, Color eyeColor)
    {
        if (IsServer) return;
        StartRampageLocally(mult, eyeColor);
    }

    private void StartRampageLocally(float mult, Color eyeColor)
    {
        if (rampageActive) return;
        rampageActive = true;

        OnRampageStart?.Invoke(mult, eyeColor);
    }

    public void EndRampage()
    {
        if (!IsServer)
        {
            Debug.LogWarning("RampageManager: only the server can end rampage.");
            return;
        }

        Debug.Log("🛑 Rampage ended.");
        EndRampageClientRpc();
        StopRampageLocally(); // server also applies locally
    }

    [ClientRpc]
    private void EndRampageClientRpc()
    {
        if (IsServer) return;
        StopRampageLocally();
    }

    private void StopRampageLocally()
    {
        if (!rampageActive) return;
        rampageActive = false;

        OnRampageEnd?.Invoke();

        if (ConsumableManager.Instance != null)
        {
            ConsumableManager.Instance.Add("Keycard", 1);
            Debug.Log("🎁 Received 1 Keycard on rampage end.");
        }
    }

    public bool IsRampageActive => rampageActive;
    public float SpeedMultiplier => speedMultiplier;
    public Color RampageEyeColor => rampageEyeColor;
}
