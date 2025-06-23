using UnityEngine;
using TMPro;
using Steamworks;

public class SteamNameDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private string fallbackName = "DemoPlayer";

    private void Start()
    {
        string displayName = fallbackName;

        if (SteamClient.IsValid)
        {
            displayName = SteamClient.Name;
        }
        else
        {
            Debug.LogWarning("⚠️ Steam not initialized. Using fallback name: " + fallbackName);
        }

        if (playerNameText != null)
        {
            playerNameText.text = displayName;
        }
    }
}
