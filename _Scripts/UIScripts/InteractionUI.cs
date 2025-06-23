using UnityEngine;
using TMPro;

/// <summary>
/// Show interactable text
/// </summary>
public class InteractionUI : MonoBehaviour
{
    private TextMeshProUGUI interactTextUI;

    [SerializeField] private Interactor Interactor;

    private void Awake()
    {
        interactTextUI = GetComponent<TextMeshProUGUI>();
        Interactor.OnInteractTextChanged += UpdateText;
    }

    private void OnDestroy()
    {
        Interactor.OnInteractTextChanged -= UpdateText;
    }

    private void UpdateText(string newText)
    {
        interactTextUI.text = newText;
    }
}
