using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows revive progress on a slider (0 to 1).
/// </summary>
public class ReviveUI : MonoBehaviour
{
    [SerializeField] private GameObject reviveUI;
    private Slider reviveSlider;

    void Awake()
    {
        reviveSlider = reviveUI.GetComponent<Slider>();
        if (reviveSlider == null)
        {
            Debug.LogError("Slider component missing from " + gameObject.name);
            return;
        }

        reviveSlider.value = 0f;
        reviveUI.SetActive(false); // Hide by default
    }

    public void Show()
    {
        reviveSlider.value = 0f;
        reviveUI.SetActive(true);
    }

    public void Hide()
    {
        reviveUI.SetActive(false);
    }

    public void SetProgress(float progress)
    {
        reviveSlider.value = Mathf.Clamp01(progress);
    }
}
