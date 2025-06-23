using UnityEngine;

public class HandPositionController : MonoBehaviour
{
    [Header("Hand Position Targets")]
    public Transform oneHandedPosition;  // Position for one-handed weapons
    public Transform twoHandedPosition;  // Position for two-handed weapons

    [SerializeField] private WeaponSwitcher weaponSwitcher;

    private void Start()
    {
        if (CurrentWeaponHolder.Instance == null)
        {
            Debug.LogError("❌ CurrentWeapon instance not found! Ensure a WeaponManager is in the scene.");
            return;
        }
        
        weaponSwitcher.OnWeaponSwitched += UpdateHandPosition;

        UpdateHandPosition();

    }

    private void OnDestroy()
    {
        weaponSwitcher.OnWeaponSwitched -= UpdateHandPosition;
    }

    public void UpdateHandPosition()
    {
        WeaponBase currentWeapon = CurrentWeaponHolder.Instance.CurrentWeapon;
        if (currentWeapon == null)
        {
            Debug.LogWarning("⚠️ No active weapon found in WeaponManager. Hand position unchanged.");
            return;
        }

        // Use the GameObject's tag to determine the appropriate hand position.
        if (currentWeapon.gameObject.CompareTag("TwoHanded"))
        {
            Debug.Log($"🟢 Switched to TWO-HANDED weapon: {currentWeapon.name}");
            MoveHandTo(twoHandedPosition);
        }
        else if (currentWeapon.gameObject.CompareTag("OneHanded"))
        {
            Debug.Log($"🟢 Switched to ONE-HANDED weapon: {currentWeapon.name}");
            MoveHandTo(oneHandedPosition);
        }
        else
        {
            Debug.LogWarning($"⚠️ Weapon '{currentWeapon.name}' has NO valid tag! Defaulting to One-Handed.");
            MoveHandTo(oneHandedPosition);
        }
    }

    private void MoveHandTo(Transform targetPosition)
    {
        if (targetPosition == null)
        {
            Debug.LogError("❌ Hand position target is not set!");
            return;
        }

        transform.position = targetPosition.position;
        transform.rotation = targetPosition.rotation;
    }
}
