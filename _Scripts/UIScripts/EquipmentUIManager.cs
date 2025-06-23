using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; } // 🧩 Singleton

    private GameObject iconPrefab;
    private Dictionary<Sprite, GameObject> activeIcons = new();

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        iconPrefab = Resources.Load<GameObject>("UI/IconPrefab");
    }

    public void DisplayIcon(Sprite icon)
    {
        if (iconPrefab == null || icon == null)
            return;

        if (activeIcons.ContainsKey(icon))
        {
            Debug.LogWarning($"⚠️ Icon for {icon.name} already displayed.");
            return;
        }

        GameObject newIcon = Instantiate(iconPrefab, transform);
        Image img = newIcon.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = icon;
        }

        activeIcons[icon] = newIcon;
    }

    public void HideIcon(Sprite icon)
    {
        if (icon == null) return;

        if (activeIcons.TryGetValue(icon, out GameObject iconObj))
        {
            Destroy(iconObj);
            activeIcons.Remove(icon);
        }
    }
}
