using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class RelayJoinButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private void Start()
    {
        joinButton.onClick.AddListener(() =>
        {
            if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
                JoinRelayWithCode(inputField.text);
        });
    }

    private async void JoinRelayWithCode(string joinCode)
    {
        joinButton.interactable = false;
        createButton.interactable = false;

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
            NetworkManager.Singleton.StartClient();

            Debug.Log("Joined host with code: " + joinCode);
        }
        catch (Exception ex)
        {
            Debug.LogError("Join failed: " + ex.Message);
            joinButton.interactable = true; // 🔁 Re-enable if it fails
            createButton.interactable = true;
        }
    }
}
