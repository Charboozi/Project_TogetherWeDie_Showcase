using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public string CurrentMapName { get; private set; } = "Space Station";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMapName(string mapName)
    {
        CurrentMapName = mapName;
        Debug.Log($"🌍 Current map set to: {mapName}");
    }
}
