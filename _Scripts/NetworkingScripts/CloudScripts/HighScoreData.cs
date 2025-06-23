using System;
using System.Collections.Generic;

[Serializable]
public class HighScoreData
{
    public List<MapHighScore> mapScores = new();

    public int GetScore(string map)
    {
        foreach (var score in mapScores)
        {
            if (score.mapName == map)
                return score.highScore;
        }
        return 0;
    }

    public void SetScore(string map, int day)
    {
        foreach (var score in mapScores)
        {
            if (score.mapName == map)
            {
                if (score.highScore < day)
                    score.highScore = day;
                return;
            }
        }
        mapScores.Add(new MapHighScore { mapName = map, highScore = day });
    }
}

[Serializable]
public class MapHighScore
{
    public string mapName;
    public int highScore;
}
