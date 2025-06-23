using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class AnnouncerVoiceManager : NetworkBehaviour
{
    public static AnnouncerVoiceManager Instance { get; private set; }

    [SerializeField] private AudioSource voiceAudioSource;

    [System.Serializable]
    public class VoiceLine
    {
        public string key;
        public AudioClip clip;
    }

    [SerializeField] private List<VoiceLine> voiceLines = new();

    private Dictionary<string, AudioClip> voiceDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Build dictionary for quick lookup
        voiceDict = new Dictionary<string, AudioClip>();
        foreach (var line in voiceLines)
        {
            if (!voiceDict.ContainsKey(line.key))
                voiceDict.Add(line.key, line.clip);
        }
    }

    void PlayVoiceLine(string key)
    {
        if (voiceAudioSource == null)
        {
            Debug.LogWarning("🔊 No AudioSource assigned to AnnouncerVoiceManager.");
            return;
        }

        if (voiceDict.TryGetValue(key, out var clip))
        {
            voiceAudioSource.Stop();
            voiceAudioSource.clip = clip;
            voiceAudioSource.Play();
        }
        else
        {
            Debug.LogWarning($"🔍 Voice line '{key}' not found!");
        }
    }

    [ClientRpc]
    public void PlayVoiceLineClientRpc(string key)
    {
        PlayVoiceLine(key);
    }

}
