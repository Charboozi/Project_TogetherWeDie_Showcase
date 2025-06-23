using UnityEngine;
using Unity.Netcode;
using TMPro;
using Steamworks;
using Unity.Collections;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNameTag : NetworkBehaviour
{
    [Header("Assign your world-space TextMeshPro here")]
    public TMP_Text nameText;

    private Transform cameraTransform;

    private NetworkVariable<FixedString64Bytes> steamName = new NetworkVariable<FixedString64Bytes>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : null;

        // Only the owner should set their name
        if (IsOwner && SteamClient.IsValid)
        {
            steamName.Value = SteamClient.Name;
        }

        steamName.OnValueChanged += OnSteamNameChanged;

        // Set initial text immediately
        nameText.text = steamName.Value.ToString();
    }

    private void OnSteamNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 direction = transform.position - cameraTransform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        nameText.transform.parent.rotation = lookRotation;
    }

    public override void OnDestroy()
    {
        steamName.OnValueChanged -= OnSteamNameChanged;
    }

    public string GetPlayerName()
    {
        return steamName.Value.ToString();
    }
}
