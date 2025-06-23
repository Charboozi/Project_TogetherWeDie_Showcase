using UnityEngine;
using UnityEngine.UI;

public class EquipmentIcon : MonoBehaviour
{
    private Image iconImage;

    public void SetIcon(Sprite sprite)
    {
        iconImage = GetComponent<Image>();
        if (iconImage != null)
            iconImage.sprite = sprite;
    }
}
