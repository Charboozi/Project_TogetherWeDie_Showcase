using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverSceneManager : MonoBehaviour
{
    public static bool OpenLobbyAfterLoad = false;

    [SerializeField] private float returnToLobbyDelay = 10f;
    [SerializeField] private string lobbySceneName = "MainMenu"; // Change to your actual scene name
    [SerializeField] private float musicFadeDuration = 3f;

    private AudioSource audioSource;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        audioSource = GetComponent<AudioSource>();

        Debug.Log($"🟥 Game Over — returning to lobby in {returnToLobbyDelay} seconds...");

        StartCoroutine(HandleSceneTransition());
    }

    private IEnumerator HandleSceneTransition()
    {
        if (audioSource != null)
        {
            // Wait until it's time to start fading music
            yield return new WaitForSeconds(returnToLobbyDelay - musicFadeDuration);
            StartCoroutine(FadeOutMusic(musicFadeDuration));
        }

        // Wait for the remaining time (music fade duration)
        yield return new WaitForSeconds(musicFadeDuration);

        OpenLobbyAfterLoad = true;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
        }
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset in case AudioSource is reused
    }
}
