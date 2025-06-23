using TMPro;
using UnityEngine;
using Unity.Netcode;

public class LobbyWagerUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField wagerInputField;
    [SerializeField] private TMP_Text wagerDisplayText;

    private void Start()
    {
        wagerInputField.onValueChanged.AddListener(OnWagerChanged);

        GameModeManager.Instance.PvPWagerAmount.OnValueChanged += OnWagerUpdated;
        UpdateDisplay(GameModeManager.Instance.PvPWagerAmount.Value);
    }

    private void OnDestroy()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.PvPWagerAmount.OnValueChanged -= OnWagerUpdated;
        }
    }

    private void OnWagerChanged(string text)
    {
        if (int.TryParse(text, out int amount))
        {
            GameModeManager.Instance.SetWagerServerRpc(amount);
        }
    }

    private void OnWagerUpdated(int oldVal, int newVal)
    {
        UpdateDisplay(newVal);
    }

    private void UpdateDisplay(int amount)
    {
        if (wagerDisplayText != null)
        {
            wagerDisplayText.text = $"Wager: {amount}";
        }
    }
}
