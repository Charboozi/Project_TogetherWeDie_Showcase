using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private AudioSource audioSource;

    private void Awake()
    {
        // Try find or create an AudioSource automatically
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip != null)
        {
            audioSource.PlayOneShot(hoverClip);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickClip != null)
        {
            audioSource.PlayOneShot(clickClip);
        }
    }
}
