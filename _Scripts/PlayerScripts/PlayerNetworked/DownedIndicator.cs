using UnityEngine;
using Unity.Netcode;

public class DownedIndicator : NetworkBehaviour
{
    [SerializeField] private GameObject skullUI;

    private EntityHealth health;

    public override void OnNetworkSpawn()
    {
        health = GetComponent<EntityHealth>();
        if (health != null)
        {
            health.isDowned.OnValueChanged += OnDownedChanged;
            skullUI.SetActive(health.isDowned.Value); // Initial state
        }
    }

    private void OnDownedChanged(bool oldValue, bool newValue)
    {
        skullUI.SetActive(newValue);
    }

    private void LateUpdate()
    {
        if (skullUI.activeSelf)
        {
            skullUI.transform.LookAt(Camera.main.transform);
        }
    }

    public override void OnDestroy()
    {
        if (health != null)
        {
            health.isDowned.OnValueChanged -= OnDownedChanged;
        }
    }
}
