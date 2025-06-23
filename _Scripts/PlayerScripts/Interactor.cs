using UnityEngine;
using System;

/// <summary>
/// Add to player to be able to interact with interactables
/// </summary>
public class Interactor : MonoBehaviour
{
    public event Action<string> OnInteractTextChanged;

    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;

    private Interactable currentInteractable;

    private void OnEnable()
    {
        PlayerInput.OnInteractPressed += TryInteract;
    }

    private void OnDisable()
    {
        PlayerInput.OnInteractPressed -= TryInteract;
    }

    private void Update()
    {
        HandleRaycast();

        if (currentInteractable != null)
        {
            OnInteractTextChanged?.Invoke(currentInteractable.GetInteractText());
        }
        else
        {
            OnInteractTextChanged?.Invoke(string.Empty);
        }
    }

    private void HandleRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (((1 << hit.collider.gameObject.layer) & interactLayer) != 0 &&
                hit.collider.TryGetComponent(out Interactable interactable))
            {
                currentInteractable = interactable;
                return;
            }
        }

        // No valid interactable hit
        currentInteractable = null;
    }

    private void TryInteract()
    {
        if (currentInteractable != null && !currentInteractable.isCoolingDown.Value)
        {
            currentInteractable.Interact();
        }
    }
}
