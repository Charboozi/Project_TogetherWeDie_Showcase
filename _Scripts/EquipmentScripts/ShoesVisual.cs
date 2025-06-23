using UnityEngine;

public class ShoesVisual : MonoBehaviour
{
    [SerializeField] private GameObject[] shoesObjects;

    private void OnEnable()
    {
        SetShoesActive(true);
    }

    private void OnDisable()
    {
        SetShoesActive(false);
    }

    private void SetShoesActive(bool isActive)
    {
        if (shoesObjects == null || shoesObjects.Length == 0)
            return;

        foreach (var shoe in shoesObjects)
        {
            if (shoe != null)
                shoe.SetActive(isActive);
        }
    }
}
