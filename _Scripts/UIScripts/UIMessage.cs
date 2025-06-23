using UnityEngine;
using TMPro;

public class UIMessage : MonoBehaviour
{
    [Tooltip("Root GameObject that holds the UI")]
    public GameObject messageRoot;

    [Tooltip("Optional text component (TextMeshPro)")]
    public TMP_Text messageText;

    public void Show(string text = null)
    {
        if (text != null && messageText != null)
            messageText.text = text;

        messageRoot.SetActive(true);
    }

    public void Hide()
    {
        messageRoot.SetActive(false);
    }
}
