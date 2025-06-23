using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponInventoryUI : MonoBehaviour
{
    [SerializeField] private WeaponInventory weaponInventory;
    [SerializeField] private GameObject weaponCirclePrefab;
    [SerializeField] private Transform circleContainer;

    private List<Image> circleImages = new List<Image>();

    private void Start()
    {
        weaponInventory.OnWeaponAdded += RedrawCircles;
        weaponInventory.OnWeaponLimitReached += (_) => RedrawCircles();

        if (WeaponSwitcher.Instance != null)
        {
            WeaponSwitcher.Instance.OnWeaponSwitched += UpdateCurrentIndicator;
        }

        RedrawCircles(); // initialize
    }

    private void RedrawCircles(WeaponBase _ = null)
    {
        foreach (var img in circleImages)
            Destroy(img.gameObject);
        circleImages.Clear();

        int count = weaponInventory.WeaponCount;
        for (int i = 0; i < count; i++)
        {
            GameObject circle = Instantiate(weaponCirclePrefab, circleContainer);
            Image img = circle.GetComponent<Image>();
            circleImages.Add(img);
        }

        UpdateCurrentIndicator();
    }

    private void UpdateCurrentIndicator()
    {
        int currentIndex = WeaponSwitcher.Instance?.CurrentWeaponIndex ?? 0;

        for (int i = 0; i < circleImages.Count; i++)
        {
            circleImages[i].color = (i == currentIndex) ? Color.white : Color.grey;
        }
    }
}
