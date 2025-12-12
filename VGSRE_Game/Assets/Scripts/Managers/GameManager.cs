using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor.XR;
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
        public float m_MinDifficulty;
        public float m_MaxDifficulty;
        public Transform m_LeftScreenLimit;
        public Transform m_RightScreenLimit;
        public GameObject[] m_SourceList;
    }

    [HideInInspector]
    public Round m_HighestRound;
    [HideInInspector]
    public Round m_CurrentRound;
    [HideInInspector]
    public float m_CurrentDifficulty = 0f;
    [HideInInspector]
    public GameObject m_CurrentSource = null;

    [SerializeField]
    Round[] m_RoundsList;

    [Header("Parameters")]
    public bool m_CanRegressRounds;
    public float m_DifficultyIncrease;
    public float m_DifficultyDecrease;

    private void Start()
    {
        PlaySound();

        m_CurrentRound = m_RoundsList[0];
        m_HighestRound = m_CurrentRound;
    }

    public void PlaySound()
    {
        m_CurrentSource = GetRandomSource();

        // PLAY SOUND CODE (SoundManager not done yet)
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

        if (m_CanRegressRounds)
            m_CurrentDifficulty = Mathf.Clamp(m_CurrentDifficulty, minPossibleDifficulty, maxPossibleDifficulty);
        else
            m_CurrentDifficulty = Mathf.Clamp(m_CurrentDifficulty, m_CurrentRound.m_MinDifficulty, maxPossibleDifficulty);

        UpdateRound();
        PlaySound();
    }

    public void UpdateRound()
    {
        foreach (var round in m_RoundsList)
        {
            if (round.m_MinDifficulty <= m_CurrentDifficulty &&
                round.m_MaxDifficulty >= m_CurrentDifficulty)
            {
                m_CurrentRound = round;
                m_HighestRound = (m_CurrentRound.m_Number > m_HighestRound.m_Number ? m_CurrentRound : m_HighestRound);
            }
        }
    }

    public GameObject GetRandomSource()
    {
        List<GameObject> allAvailableSources = new List<GameObject>();

        for (int i = 0; i <= m_CurrentRound.m_Number; i++)
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

    public bool WillBeWithinScreenLimits(Vector3 newCameraPosition)
    {
        float halfScreenDistance = Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)),
                                                    Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0f, 0f)));

        if (m_CanRegressRounds)
            return (newCameraPosition.x >= (m_HighestRound.m_LeftScreenLimit.position.x + halfScreenDistance) &&
                    newCameraPosition.x <= (m_HighestRound.m_RightScreenLimit.position.x - halfScreenDistance));
        else
            return (newCameraPosition.x >= (m_CurrentRound.m_LeftScreenLimit.position.x + halfScreenDistance) &&
                    newCameraPosition.x <= (m_CurrentRound.m_RightScreenLimit.position.x - halfScreenDistance));
    }
}
