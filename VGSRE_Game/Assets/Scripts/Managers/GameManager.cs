using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

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

    //////////////////////////////////////////

    [Serializable]
    public struct Round
    {
        public int m_Number;
        public int m_MinDifficulty;
        public float m_MaxDifficulty;
        public GameObject[] m_SourceList;
    }

    [HideInInspector]
    public int m_CurrentRound = 0;
    [HideInInspector]
    public float m_CurrentDifficulty = 0f;
    [HideInInspector]
    public GameObject m_CurrentSource = null;

    [SerializeField]
    Round[] m_RoundsList;

    [Header("Parameters")]
    public float m_DifficultyIncrease;
    public float m_DifficultyDecrease;

    private void Start()
    {
        PlaySound();
    }

    public void PlaySound()
    {
        m_CurrentSource = GetRandomSource();

        // PLAY SOUND CODE
    }

    public void GuessSound(GameObject sourceGuess)
    {
        bool isCorrectSource = (sourceGuess == m_CurrentSource);

        if (isCorrectSource)
        {
            m_CurrentDifficulty += m_DifficultyIncrease;
        }
        else
        {
            m_CurrentDifficulty -= m_DifficultyDecrease;
        }

        float minPossibleDifficulty = m_RoundsList[0].m_MinDifficulty;
        float maxPossibleDifficulty = m_RoundsList[m_RoundsList.Length - 1].m_MaxDifficulty;

        m_CurrentDifficulty = Mathf.Clamp(m_CurrentDifficulty, minPossibleDifficulty, maxPossibleDifficulty);

        UpdateRound();
        PlaySound();
    }

    public void UpdateRound()
    {
        foreach (var round in m_RoundsList)
        {
            if (round.m_MinDifficulty < m_CurrentDifficulty &&
                round.m_MaxDifficulty >= m_CurrentDifficulty)
            {
                m_CurrentRound = round.m_Number;
            }
        }
    }

    public GameObject GetRandomSource()
    {
        List<GameObject> allAvailableSources = new List<GameObject>();

        for (int i = 0; i <= m_CurrentRound; i++)
        {
            foreach (GameObject roundSource in m_RoundsList[i].m_SourceList)
            {
                allAvailableSources.Add(roundSource);
            }
        }

        var rand = new System.Random();
        int sourceIndex = rand.Next(0, allAvailableSources.Count);

        return allAvailableSources[sourceIndex];
    }
}
