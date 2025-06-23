using UnityEngine;
using System;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public static event Action<bool> OnPauseStateChanged;

    private void OnEnable()
    {
        PlayerInput.OnPausePressed += TogglePause;
    }

    private void OnDisable()
    {
        PlayerInput.OnPausePressed -= TogglePause;
    }

    private void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsPaused = true;
        OnPauseStateChanged?.Invoke(true);
    }

    private void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsPaused = false;
        OnPauseStateChanged?.Invoke(false);
    }

}
