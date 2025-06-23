using UnityEngine;

public class ShopMenuOpener : MonoBehaviour
{
    [SerializeField] private KeycardShop keycardShop;

    public void OpenShop()
    {
        keycardShop.UpdateUI(); // ✅ Update UI on demand
    }

}
