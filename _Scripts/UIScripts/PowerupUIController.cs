using UnityEngine;
using System.Collections;

public class PowerupUIController : MonoBehaviour
{
    public static PowerupUIController Instance { get; private set; }

    [Header("Powerup Icons")]
    [SerializeField] private GameObject armorBoostIcon;
    [SerializeField] private GameObject speedBoostIcon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        armorBoostIcon?.SetActive(false);
        speedBoostIcon?.SetActive(false);
    }

    public void ShowArmorBoost(float duration)
    {
        if (armorBoostIcon == null) return;
        StartCoroutine(ShowTemporarily(armorBoostIcon, duration));
    }

    public void ShowSpeedBoost(float duration)
    {
        if (speedBoostIcon == null) return;
        StartCoroutine(ShowTemporarily(speedBoostIcon, duration));
    }

    private IEnumerator ShowTemporarily(GameObject icon, float duration)
    {
        icon.SetActive(true);
        yield return new WaitForSeconds(duration);
        icon.SetActive(false);
    }
}
