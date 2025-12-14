using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    ///////////////////////////////////////////////////////

    [Serializable]
    public class SaveObject
    {
        public float totalPlayTime;
        public int levelReached;
        public int tries;
        public int successfulTries;
        public int failedTries;
        public List<float> levelTries;
        public List<float> levelSuccessfulTries;
        public List<float> levelSuccessRate;
        public float totalSuccessRate;
        public float averageSoundLocalizationTime;
        public int maxSequenceLength;
        public int correctSounds;
    }

    public void SaveData()
    {
        string folderPath = Path.Combine(Application.dataPath, "Saves");
        string path = "";

        for (int i = 0; i < 1000; i++)
        {
            if (!File.Exists(Path.Combine(folderPath, $"SavedData{i}.txt")))
            {
                path = Path.Combine(folderPath, $"SavedData{i}.txt");
                break;
            }
        }

        GameManager gameManager = GameManager.instance;

        float reactionTimesSum = 0f;
        foreach (float time in gameManager.m_SoundReactionTimes)
        {
            reactionTimesSum += time;
        }

        float averageReactionTime = (reactionTimesSum / gameManager.m_SoundReactionTimes.Count);

        SaveObject saveObject = new SaveObject
        {
            totalPlayTime = (gameManager.m_TotalTime - gameManager.m_RemainingTime),
            levelReached = gameManager.m_CurrentLevel,
            tries = gameManager.m_TotalTries,
            successfulTries = gameManager.m_TotalSuccessfulTries,
            failedTries = gameManager.m_TotalFailedTries,
            levelTries = gameManager.m_LevelTries,
            levelSuccessfulTries = gameManager.m_LevelSuccessfulTries,
            levelSuccessRate = gameManager.m_LevelSuccessRate,
            totalSuccessRate = (gameManager.m_TotalSuccessfulTries / gameManager.m_TotalTries),
            averageSoundLocalizationTime = averageReactionTime,
            maxSequenceLength = gameManager.m_MaxSequenceLength,
            correctSounds = gameManager.m_TotalSuccessfulSounds
        };

        string json = JsonUtility.ToJson(saveObject);
        File.WriteAllText(path, json);
    }
}
