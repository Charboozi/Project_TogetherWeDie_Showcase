using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class ArmsPositionController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 basePosition;
    private Quaternion baseRotation;
    private List<IArmsOffsetProvider> offsetProviders = new();
    private Transform weaponTransform;

    void Awake()
    {
        weaponTransform = transform;
        basePosition = weaponTransform.localPosition;
        baseRotation = weaponTransform.localRotation;

        offsetProviders.AddRange(GetComponents<IArmsOffsetProvider>());
    }

    void Update()
    {
        Vector3 totalOffset = Vector3.zero;
        Quaternion totalRotation = Quaternion.identity;

        foreach (var provider in offsetProviders)
        {
            totalOffset += provider.GetOffset();
            totalRotation *= provider.GetRotation(); // important
        }

        Vector3 targetPosition = basePosition + totalOffset;
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);

        Quaternion targetRotation = baseRotation * totalRotation;
        weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}
