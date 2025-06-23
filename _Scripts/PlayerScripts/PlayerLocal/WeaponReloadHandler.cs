using UnityEngine;
using System.Collections;

public class WeaponReloadHandler : MonoBehaviour
{
    public Coroutine ReloadCoroutine { get; private set; }
    public Coroutine DelayCoroutine { get; private set; }

    [SerializeField] private UIMessage rechargeMessage;
    
    private WeaponRechargeAudioHandler audioHandler;
    private WeaponBase weapon;

    void Awake()
    {
        audioHandler = GetComponent<WeaponRechargeAudioHandler>();
    }

    public void Initialize(WeaponBase currentWeapon)
    {
        weapon = currentWeapon;
        Debug.Log($"ReloadHandler initialized with weapon: {weapon.name}");
    }

    public void CancelReloads()
    {
        rechargeMessage?.Hide();
        audioHandler?.StopChargingLoop();

        if (DelayCoroutine != null)
        {
            StopCoroutine(DelayCoroutine);
            DelayCoroutine = null;
        }

        if (ReloadCoroutine != null)
        {
            StopCoroutine(ReloadCoroutine);
            ReloadCoroutine = null;
            WeaponController.OnReloadEnd?.Invoke();
        }
    }

    public void TryAutoReload()
    {
        if (weapon == null) return; // ✅ Prevent NullRef
        if (DelayCoroutine == null && weapon.currentAmmo < weapon.maxAmmo)
        {
            rechargeMessage?.Show("RECHARGING...");
            DelayCoroutine = StartCoroutine(DelayBeforeReload());
        }
    }

    private IEnumerator DelayBeforeReload()
    {
        float interval = weapon.reloadDuration / weapon.maxAmmo;
        yield return new WaitForSeconds(interval);
        DelayCoroutine = null;
        ReloadCoroutine = StartCoroutine(DoReload());
    }

    private IEnumerator DoReload()
    {
        WeaponController.OnReloadStart?.Invoke();
        audioHandler?.PlayChargingLoop();

        while (weapon.currentAmmo < weapon.maxAmmo && weapon.reserveAmmo > 0)
        {
            yield return new WaitForSeconds(0.1f);
            weapon.currentAmmo++;
            weapon.reserveAmmo--;
            weapon.UpdateEmissionIntensity();
        }

        rechargeMessage?.Hide();
        audioHandler?.StopChargingLoop();
        WeaponController.OnReloadEnd?.Invoke();
        ReloadCoroutine = null;
    }

    public bool IsReloading => ReloadCoroutine != null;
}
