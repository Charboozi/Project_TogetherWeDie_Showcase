using UnityEngine;
using TMPro;

public class DayUI : MonoBehaviour
{
    private TextMeshProUGUI dayText;

    private void Start()
    {
        dayText = GetComponent<TextMeshProUGUI>();

        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted += UpdateDayText;

            UpdateDayText(DayManager.Instance.CurrentDayInt);
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted -= UpdateDayText;
        }
    }

    private void UpdateDayText(int dayNumber)
    {
        dayText.text = $"Day {dayNumber}";
    }
}
