using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MusicPlayer : NetworkBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [System.Serializable]
    public class NamedMusicClip
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<NamedMusicClip> musicClips;

    private Dictionary<string, AudioClip> musicLibrary;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        musicLibrary = new Dictionary<string, AudioClip>();
        foreach (var entry in musicClips)
        {
            if (!musicLibrary.ContainsKey(entry.name))
                musicLibrary.Add(entry.name, entry.clip);
        }
    }

    public void ServerPlayMusic(string name)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can start music.");
            return;
        }

        if (!musicLibrary.ContainsKey(name))
        {
            Debug.LogWarning($"MusicPlayer: No clip found with name '{name}'");
            return;
        }

        // Instruct clients to play the clip
        PlayMusicClientRpc(name);
    }

    [ClientRpc]
    private void PlayMusicClientRpc(string clipName)
    {
        if (!musicLibrary.TryGetValue(clipName, out AudioClip clip))
        {
            Debug.LogWarning($"MusicPlayer: Missing clip '{clipName}' on client.");
            return;
        }

        if (audioSource.isPlaying && audioSource.clip == clip)
            return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayMusicServerRpc(string clipName)
    {
        ServerPlayMusic(clipName);
    }

    public void StopMusic()
    {
        if (!audioSource) return;
        audioSource.Stop();
        audioSource.clip = null;
    }

    public bool IsPlaying() => audioSource.isPlaying;
}
