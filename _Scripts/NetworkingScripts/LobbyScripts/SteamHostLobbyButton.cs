using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using System;

[RequireComponent(typeof(Button))]
public class SteamHostLobbyButton : MonoBehaviour
{
    public enum GameModeType { PvP, CoOp }

    [SerializeField] private GameModeType selectedGameMode; // Set this via Inspector or another button
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private Button createButton;
    [SerializeField] private Button inviteButton;

    private Lobby currentLobby;

    public async void CreateLobby()
    {
        createButton.interactable = false;

        try
        {
            var result = await SteamMatchmaking.CreateLobbyAsync(5);
            if (result.HasValue)
            {
                currentLobby = result.Value;
                currentLobby.SetJoinable(true);

                // Set metadata
                currentLobby.SetData("hostId", SteamClient.SteamId.ToString());
                currentLobby.SetData("gameMode", selectedGameMode.ToString()); // Store PvP or CoOp

                // Advertise lobby via Rich Presence so friends see "Join Game"
                SteamFriends.SetRichPresence("connect", currentLobby.Id.ToString());
                SteamFriends.SetRichPresence("status", "In Lobby");

                if (codeDisplay != null)
                    codeDisplay.text = SteamClient.Name;

                NetworkManager.Singleton.StartHost();
                Debug.Log($"✅ Hosting Steam lobby. HostID: {SteamClient.SteamId}, Mode: {selectedGameMode}");

                // Hook up invite button for further invites
                if (inviteButton != null)
                    inviteButton.onClick.AddListener(() => SteamFriends.OpenGameInviteOverlay(currentLobby.Id));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Failed to create Steam lobby: " + ex.Message);
            createButton.interactable = true;
        }
    }

}
