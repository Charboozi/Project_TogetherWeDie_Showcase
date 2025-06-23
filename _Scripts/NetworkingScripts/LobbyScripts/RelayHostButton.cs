using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System;

[RequireComponent(typeof(Button))]
public class RelayHostButton : MonoBehaviour
{
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private Button createButton;

    public void CreateLobby()
    {
        CreateRelayHost();
    }

    // Optional manual relay host creator
    private async void CreateRelayHost()
    {
        createButton.interactable = false;

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(5);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            if (codeDisplay != null)
                codeDisplay.text = joinCode;

            Debug.Log("Relay host started with code: " + joinCode);
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Host creation failed: " + ex.Message);
            createButton.interactable = true;
        }
    }
}
