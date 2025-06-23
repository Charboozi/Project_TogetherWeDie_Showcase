using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;

    private void OnEnable()
    {
        PauseManager.OnPauseStateChanged += HandlePauseState;
    }

    private void OnDisable()
    {
        PauseManager.OnPauseStateChanged -= HandlePauseState;
    }

    private void HandlePauseState(bool isPaused)
    {
        pauseMenuUI.SetActive(isPaused);
    }

    public void OnQuitButton()
    {
        // Replace with scene load or quit logic
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
