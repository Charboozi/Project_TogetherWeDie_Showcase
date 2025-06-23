using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance;
    private string savePath => Application.persistentDataPath + "/highscores.json";
    private HighScoreData data = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadScores();
    }

    public int GetHighScore(string mapName)
    {
        return data.GetScore(mapName);
    }

    public void SetHighScore(string mapName, int day)
    {
        Debug.Log($"📥 Request to set high score for map: {mapName}, day: {day}");

        int existing = data.GetScore(mapName);
        Debug.Log($"🧾 Existing score: {existing}");

        if (existing < day)
        {
            data.SetScore(mapName, day);
            SaveScores();
            Debug.Log($"✅ New high score saved: Day {day}");
        }
        else
        {
            Debug.Log("⛔ Not updating, score is not higher.");
        }
    }

    private void SaveScores()
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 High scores saved at {savePath}:\n{json}");
    }

    private void LoadScores()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);

            data = JsonUtility.FromJson<HighScoreData>(json);
            if (data == null)
            {
                Debug.LogError("❌ Failed to parse JSON.");
                data = new HighScoreData();
            }
            else
            {
                Debug.Log("🔄 Loaded high scores:");
                foreach (var score in data.mapScores)
                {
                    Debug.Log($"📍 {score.mapName} → Day {score.highScore}");
                }
            }
        }
        else
        {
            Debug.Log("🆕 No high score file found. Starting fresh.");
            data = new HighScoreData();
        }
    }
}
