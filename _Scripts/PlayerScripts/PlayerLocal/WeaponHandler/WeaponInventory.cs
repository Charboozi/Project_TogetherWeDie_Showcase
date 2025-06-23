using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CurrentWeaponHolder))]
public class WeaponInventory : MonoBehaviour
{
    private int baseMaxWeapons = 2;
    private int currentMaxWeapons;

    [SerializeField]private List<WeaponBase> ownedWeapons = new List<WeaponBase>();

    public IReadOnlyList<WeaponBase> Weapons => ownedWeapons;

    public event System.Action<WeaponBase> OnWeaponAdded;
    public event System.Action<int> OnWeaponLimitReached;

    void Awake()
    {
        currentMaxWeapons = baseMaxWeapons;
        
        if (!GameModeManager.Instance.IsPvPMode)
        {
            EnsureAtLeastOneWeapon();
        } 
    }

    public void SetMaxWeapons(int amount)
    {
        currentMaxWeapons = amount;
    }

    public void AddWeapon(WeaponBase weapon)
    {
        if (ownedWeapons.Contains(weapon)) return;

        if (ownedWeapons.Count >= currentMaxWeapons)
        {
            OnWeaponLimitReached?.Invoke(currentMaxWeapons); 
        }

        ownedWeapons.Add(weapon);
        OnWeaponAdded?.Invoke(weapon);
    }

    public void PickUpWeapon(string weaponName)
    {
        WeaponBase weaponToAdd = GetComponentsInChildren<WeaponBase>(true)
            .FirstOrDefault(w => w.name == weaponName);

        if (weaponToAdd == null) return;
        if (ownedWeapons.Contains(weaponToAdd)) return;

        AddWeapon(weaponToAdd);
    }

    public void RemoveWeapon(int index)
    {
        if (index >= 0 && index < ownedWeapons.Count)
        {
            WeaponBase weapon = ownedWeapons[index];
            ownedWeapons.RemoveAt(index);
            weapon.gameObject.SetActive(false);
        }
    }

    public void EnsureAtLeastOneWeapon()
    {
        if (ownedWeapons.Count == 0)
        {
            Debug.Log("🔫 No active weapons found. Adding fallback: Pistol.");
            PickUpWeapon("Pistol");
        }
    }

    public int WeaponCount => ownedWeapons.Count;
}
