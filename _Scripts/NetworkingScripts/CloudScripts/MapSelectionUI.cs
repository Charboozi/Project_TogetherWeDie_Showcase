using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Big map button UI")]
    [SerializeField] private Button bigMapButton;
    [SerializeField] private Image bigMapImage;

    [Header("Selection panel")]
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private Transform mapButtonContainer;
    [SerializeField] private GameObject mapButtonPrefab;

    [System.Serializable]
    public class MapInfo
    {
        public string mapName;
        public Sprite mapImage;
        public int price;
    }

    [Header("Maps Config")]
    public List<MapInfo> availableMaps;

    private void Start()
    {
        bigMapButton.onClick.AddListener(ToggleMapPanel);
        RefreshBigButton();

        foreach (var map in availableMaps)
        {
            GameObject newButton = Instantiate(mapButtonPrefab, mapButtonContainer);
            TMP_Text text = newButton.GetComponentInChildren<TMP_Text>();
            Image img = newButton.GetComponentInChildren<Image>();
            Button btn = newButton.GetComponent<Button>();

            //img.sprite = map.mapImage;

            UpdateButtonUI(btn, text, map);
        }


        mapSelectionPanel.SetActive(false);
    }

    private void UpdateButtonUI(Button btn, TMP_Text text, MapInfo map)
    {
        btn.onClick.RemoveAllListeners(); // Prevent duplicate listeners

        bool unlocked = PlayerInventoryManager.Instance.UnlockedMaps.Contains(map.mapName);

        if (unlocked)
        {
            text.text = map.mapName;
            btn.interactable = true;

            btn.onClick.AddListener(() =>
            {
                MapManager.Instance.SetMapName(map.mapName);
                RefreshBigButton();
                ToggleMapPanel();
            });
        }
        else
        {
            text.text = $"{map.mapName} - <color=#32CD32>${map.price}</color>";
            btn.interactable = true;

            btn.onClick.AddListener(() =>
            {
                if (CurrencyManager.Instance.Spend(map.price))
                {
                    PlayerInventoryManager.Instance.UnlockedMaps.Add(map.mapName);
                    PlayerInventoryManager.Instance.SaveInventory();

                    // Update button to no longer be buyable
                    UpdateButtonUI(btn, text, map);

                    MapManager.Instance.SetMapName(map.mapName);
                    RefreshBigButton();
                    ToggleMapPanel();
                    Debug.Log($"🛒 Bought map: {map.mapName}");
                }
            });
        }
    }


    private void RefreshBigButton()
    {
        string currentMap = MapManager.Instance.CurrentMapName;
        var mapData = availableMaps.Find(m => m.mapName == currentMap);
        if (mapData != null)
            bigMapImage.sprite = mapData.mapImage;
    }

    private void ToggleMapPanel()
    {
        mapSelectionPanel.SetActive(!mapSelectionPanel.activeSelf);
    }
}
