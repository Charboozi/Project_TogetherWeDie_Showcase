using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class CopyJoinCodeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(CopyCodeToClipboard);
    }

    private void CopyCodeToClipboard()
    {
        if (codeText == null || string.IsNullOrEmpty(codeText.text))
        {
            Debug.LogWarning("No code to copy!");
            return;
        }

        // Remove any prefix like "JOIN CODE: " if present
        string code = codeText.text.Replace("JOIN CODE:", "").Trim();

        GUIUtility.systemCopyBuffer = code;
        Debug.Log("Copied to clipboard: " + code);
    }
}
