using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private Light flashlight; // assign your flashlight (e.g., child light object)

    private void OnEnable()
    {
        PlayerInput.OnFlashlightToggle += ToggleFlashlight;
    }

    private void OnDisable()
    {
        PlayerInput.OnFlashlightToggle -= ToggleFlashlight;
    }

    private void ToggleFlashlight()
    {
        if (flashlight != null)
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}
