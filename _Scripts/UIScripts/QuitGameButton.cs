using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitGameButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(QuitGame);
    }

    private void QuitGame()
    {
        Debug.Log("🛑 Quit game requested");

#if UNITY_EDITOR
        // ✅ Stop play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ✅ Quit standalone build
        Application.Quit();
#endif
    }
}
