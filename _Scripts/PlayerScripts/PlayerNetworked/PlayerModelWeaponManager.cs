using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Collections;

public class PlayerModelWeaponManager : NetworkBehaviour
{
    private WeaponSwitcher weaponSwitcher;
    public Transform weaponParent; // Parent object containing weapon slots

    // This NetworkVariable stores the active weapon's name as a FixedString32Bytes.
    // Write permission is set to Server so only the server can update it.
    private NetworkVariable<FixedString32Bytes> activeWeaponName = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindWeaponSwitcher();

        if (IsClient)
        {
            activeWeaponName.OnValueChanged += OnActiveWeaponNameChanged;
        }

        // On the server, set the initial weapon name
        if (IsServer)
        {
            SetInitialWeaponName();
        }
        UpdateWeaponSlot();
    }

    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnWeaponSwitched -= RequestWeaponUpdate;
        }

        if (IsClient)
        {
            activeWeaponName.OnValueChanged -= OnActiveWeaponNameChanged;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindWeaponSwitcher();
        UpdateWeaponSlot();
    }

    void FindWeaponSwitcher()
    {
        if (weaponSwitcher == null)
        {
            weaponSwitcher = FindFirstObjectByType<WeaponSwitcher>();
            if (weaponSwitcher != null)
            {
                // Subscribe to weapon switch events
                weaponSwitcher.OnWeaponSwitched -= RequestWeaponUpdate;
                weaponSwitcher.OnWeaponSwitched += RequestWeaponUpdate;
            }
        }
    }

    // Iterate through the children of weaponParent and activate only the slot whose name matches the active weapon name.
    void UpdateWeaponSlot()
    {
        if (weaponParent == null) return;

        string weaponToActivate = activeWeaponName.Value.ToString(); // Convert FixedString32Bytes to string

        for (int i = 0; i < weaponParent.childCount; i++)
        {
            Transform weaponSlot = weaponParent.GetChild(i);
            bool shouldBeActive = (weaponSlot.gameObject.name == weaponToActivate);
            if (weaponSlot.gameObject.activeSelf != shouldBeActive)
            {
                weaponSlot.gameObject.SetActive(shouldBeActive);
            }
        }
    }

    // Called locally by the WeaponSwitcher when the player switches weapons.
    // The owner sends a ServerRpc to update the active weapon name.
    void RequestWeaponUpdate()
    {
        if (IsOwner && weaponSwitcher != null && CurrentWeaponHolder.Instance != null && CurrentWeaponHolder.Instance.CurrentWeapon != null)
        {
            string newName = CurrentWeaponHolder.Instance.CurrentWeapon.gameObject.name;
            if (newName != activeWeaponName.Value.ToString())
            {
                UpdateWeaponNameServerRpc(new FixedString32Bytes(newName));
            }
        }
    }

    // ServerRpc: Only the server writes to activeWeaponName.
    [ServerRpc(RequireOwnership = false)]
    void UpdateWeaponNameServerRpc(FixedString32Bytes newWeaponName, ServerRpcParams rpcParams = default)
    {
        activeWeaponName.Value = newWeaponName;
        // Notify the owner via a ClientRpc; note we no longer write on the client side.
        UpdateWeaponNameClientRpc(newWeaponName, rpcParams.Receive.SenderClientId);
    }

    // ClientRpc: For the owner, simply update the slot.
    [ClientRpc]
    void UpdateWeaponNameClientRpc(FixedString32Bytes newWeaponName, ulong clientId)
    {
        if (OwnerClientId == clientId)
        {
            // Do not write to activeWeaponName here since it is already synced.
            UpdateWeaponSlot();
        }
    }

    // When activeWeaponName changes, update the weapon slot.
    void OnActiveWeaponNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        UpdateWeaponSlot();
    }

    // Server-only: Set the initial active weapon name based on WeaponManager.
    void SetInitialWeaponName()
    {
        if (CurrentWeaponHolder.Instance != null && CurrentWeaponHolder.Instance.CurrentWeapon != null && weaponParent != null)
        {
            string currentName = CurrentWeaponHolder.Instance.CurrentWeapon.gameObject.name;
            activeWeaponName.Value = new FixedString32Bytes(currentName);
        }
    }
}
