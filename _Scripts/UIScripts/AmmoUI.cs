using UnityEngine;
using TMPro;

/// <summary>
/// Update Ammo UI
/// </summary>
public class AmmoUI : MonoBehaviour
{
    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI reserveAmmoText;

    void Update()
    {
        WeaponBase activeWeapon = CurrentWeaponHolder.Instance != null ? CurrentWeaponHolder.Instance.CurrentWeapon : null;
        if (activeWeapon != null)
        {
            currentAmmoText.text = activeWeapon.currentAmmo.ToString();
            reserveAmmoText.text = activeWeapon.reserveAmmo.ToString();
        }
    }
}
