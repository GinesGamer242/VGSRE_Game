using System;
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
    public class SourceListStruct
    {
        public GameObject[] m_SourceList;

        public GameObject GetRandomSource()
        {
            var rand = new System.Random();
            int sourceIndex = rand.Next(0, m_SourceList.Length);

            return m_SourceList[sourceIndex];
        }
    }

    int m_CurrentRound = 0;
    float m_CurrentDifficulty = 0f;
    GameObject m_CurrentSource = null;

    [SerializeField]
    SourceListStruct[] m_RoundSourceLists;

    [Header("Parameters")]
    public float m_RoundAmount;
    public float m_DifficultyIncrease;
    public float m_DifficultyDecrease;
    [Tooltip("The difficulty level achievable at the round before stepping into the next round.")]
    public float[] m_RoundDifficulty;
 
    public void PlaySound()
    {
        var rand = new System.Random();
        int newSourceRound = rand.Next(0, m_CurrentRound + 1);
        
        m_CurrentSource = m_RoundSourceLists[newSourceRound].GetRandomSource();
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
            m_CurrentDifficulty += m_DifficultyIncrease;
        }
    }

    public void CalculateRound()
    {

    }
}
