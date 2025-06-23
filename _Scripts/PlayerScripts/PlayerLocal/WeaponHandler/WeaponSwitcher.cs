using UnityEngine;
using System.Linq;

[RequireComponent(typeof(WeaponInventory))]
public class WeaponSwitcher : MonoBehaviour
{
    public static WeaponSwitcher Instance { get; private set; }

    public int CurrentWeaponIndex { get; private set; } = 0;

    [SerializeField] private Transform rightHand;
    [SerializeField] private WeaponController weaponController;

    [Header("Audio")]
    [SerializeField] private AudioClip switchSound;
    [SerializeField] private AudioSource audioSource;
    
    public event System.Action OnWeaponSwitched;

    private WeaponInventory inventory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        inventory = GetComponent<WeaponInventory>();
        inventory.OnWeaponAdded += HandleWeaponAdded;
        inventory.OnWeaponLimitReached += HandleWeaponLimitReached;
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnWeaponAdded -= HandleWeaponAdded;
            inventory.OnWeaponLimitReached -= HandleWeaponLimitReached;
        }
    }

    private void OnEnable()
    {
        PlayerInput.OnSwitchWeapon += EquipWeapon;
        PlayerInput.OnCycleWeapon += CycleWeapon;
    }

    private void OnDisable()
    {
        PlayerInput.OnSwitchWeapon -= EquipWeapon;
        PlayerInput.OnCycleWeapon -= CycleWeapon;
    }

    void Start()
    {
        foreach (WeaponBase weapon in inventory.Weapons)
        {
            if (weapon != null)
                weapon.gameObject.SetActive(false);
        }

        if (inventory.Weapons.Count > 0)
            EquipWeapon(0);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= inventory.Weapons.Count)
        {
            Debug.LogWarning("Weapon index out of range.");
            return;
        }

        if (rightHand != null)
        {
            rightHand.SetParent(inventory.Weapons[index].transform);
        }

        foreach (WeaponBase weapon in inventory.Weapons)
        {
            if (weapon != null)
                weapon.gameObject.SetActive(false);
        }

        inventory.Weapons[index].gameObject.SetActive(true);
        CurrentWeaponIndex = index;

        if (CurrentWeaponHolder.Instance != null)
        {
            CurrentWeaponHolder.Instance.SetWeapon(inventory.Weapons[index]);
        }

        weaponController.OnWeaponSwitched();

        Debug.Log("Equipped weapon: " + inventory.Weapons[index].name);
        OnWeaponSwitched?.Invoke();
        if (audioSource != null && switchSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = switchSound;
            audioSource.Play();
        }
    }

    public void CycleWeapon(int direction)
    {
        if (inventory.Weapons.Count == 0) return;
        int newIndex = (CurrentWeaponIndex + direction + inventory.Weapons.Count) % inventory.Weapons.Count;
        EquipWeapon(newIndex);
    }

    private void HandleWeaponLimitReached(int max)
    {
        inventory.RemoveWeapon(CurrentWeaponIndex);
    }

    private void HandleWeaponAdded(WeaponBase newWeapon)
    {
        int index = inventory.Weapons.ToList().IndexOf(newWeapon);
        EquipWeapon(index);
    }
}
