using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.Rendering;

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
        public Loop[] m_Loops;
        public Transform m_LeftScreenLimit;
        public Transform m_RightScreenLimit;
        public GameObject[] m_SourceList;
    }

    [Serializable]
    public struct Loop
    {
        public LoopSequence[] m_LoopSequences;
    }

    [Serializable]
    public class LoopSequence
    {
        public int m_SequenceLength;
        public bool m_RepeatsSequence;
        public float m_RepetitionTime;

        public LoopSequence(int thisSequenceLength, bool thisRepeatsSequence, float thisRepetitionTime)
        {
            m_SequenceLength = thisSequenceLength;
            m_RepeatsSequence = thisRepeatsSequence;
            m_RepetitionTime = thisRepetitionTime;
        }
    }


    [HideInInspector]
    public Round m_HighestRound;
    [HideInInspector]
    public Round m_CurrentRound;
    [HideInInspector]
    public int m_CurrentLevel = 0;
    [HideInInspector]
    public int m_CurrentTries = 0;
    [HideInInspector]
    public int m_CurrentSuccessfulTries = 0;
    [HideInInspector]
    public int m_CurrentSequenceIndex = 0;
    [HideInInspector]
    public List<GameObject> m_CurrentSequence = null;
    [HideInInspector]
    public LoopSequence m_CurrentLoopSequence = null;
    [HideInInspector]
    public Coroutine m_CurrentSequenceCoroutine = null;
    [HideInInspector]
    public float m_RemainingTime = 0f;

    [HideInInspector]
    public int m_TotalTries = 0;
    [HideInInspector]
    public int m_TotalSuccessfulTries = 0;
    [HideInInspector]
    public int m_TotalFailedTries = 0;
    [HideInInspector]
    public int m_TotalSuccessfulSounds = 0;
    [HideInInspector]
    public List<float> m_LevelTries = new List<float>();
    [HideInInspector]
    public List<float> m_LevelSuccessfulTries = new List<float>();
    [HideInInspector]
    public List<float> m_LevelSuccessRate = new List<float>();
    [HideInInspector]
    public float m_ChronometerTime = 0f;
    [HideInInspector]
    public List<float> m_SoundReactionTimes = new List<float>();
    [HideInInspector]
    public Coroutine m_CurrentChronometerCoroutine = null;
    [HideInInspector]
    public int m_MaxSequenceLength = 0;

    [SerializeField]
    AudioSource m_CameraAudioSource;
    [SerializeField]
    Round[] m_RoundsList;
    [SerializeField]
    LoopSequence m_FinalLoopSequence;

    [Header("Parameters")]
    public bool m_CanRegressRounds;
    [Tooltip("E.G: 100% is represented as 1")]
    public float m_SuccessRateThreshold;
    public float m_TimeBetweenSequenceAudios;
    public float m_TotalTime;

    private void Start()
    {
        m_CurrentRound = m_RoundsList[0];
        m_HighestRound = m_CurrentRound;
        m_RemainingTime = m_TotalTime;

        StartCoroutine(RoundProcessorCoroutine());
        StartCoroutine(TimerCoroutine());
    }

    public List<GameObject> BuildSequence(int length)
    {
        List<GameObject> newSequence = new List<GameObject>();
        GameObject lastSource = null;

        for (int i = 0; i < length; i++)
        {
            GameObject newSource = null;

            do
            {
                newSource = GetRandomSource();
            }
            while (newSource == lastSource);


            newSequence.Add(newSource);
        }

        return newSequence;
    }

    public void GuessSound(GameObject sourceGuess)
    {
        StopChronometerCoroutine();

        if (sourceGuess == m_CurrentSequence[m_CurrentSequenceIndex])
        {
            m_CurrentSequenceIndex++;

            m_TotalSuccessfulSounds++;
        }
        else
        {
            m_CurrentTries++;

            m_TotalTries++;
            m_TotalFailedTries++;

            m_CurrentSequenceIndex = 0;
            PlaySequence(m_CurrentSequence, m_CurrentLoopSequence);
            PlayChronometerCoroutine();
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

    float CalculateSuccessRate(int successfulTries, int totalTries)
    {
        return (successfulTries / totalTries);
    }

    void PlaySequence(List<GameObject> sequence, LoopSequence loopSequence)
    {
        if (m_CurrentSequenceCoroutine != null)
        {
            StopCoroutine(m_CurrentSequenceCoroutine);
        }
        m_CurrentSequenceCoroutine = StartCoroutine(PlaySequenceCoroutine(sequence, loopSequence));
    }

    IEnumerator PlaySequenceCoroutine(List<GameObject> sequence, LoopSequence loopSequence)
    {
        do
        {
            foreach (GameObject audioSource in sequence)
            {
                if (audioSource.TryGetComponent<SourceBehaviour>(out SourceBehaviour sourceBehaviour))
                {
                    Debug.Log(audioSource);
                    m_CameraAudioSource.resource = sourceBehaviour.GetAudioClip();
                    m_CameraAudioSource.Play();
                    yield return new WaitForSeconds(m_CameraAudioSource.clip.length + m_TimeBetweenSequenceAudios);
                }
            }

            if (loopSequence.m_RepeatsSequence)
                yield return new WaitForSeconds(loopSequence.m_RepetitionTime - m_TimeBetweenSequenceAudios);
            Debug.Log($"Sequence done ({loopSequence.m_SequenceLength}, {loopSequence.m_RepeatsSequence}, {loopSequence.m_RepetitionTime})");
        }
        while (loopSequence.m_RepeatsSequence);
    }

    IEnumerator RoundProcessorCoroutine()
    {
        int round = 0;
        int loop = 0;
        int loopSequence = 0;

        for (round = 0; round < m_RoundsList.Length; round++)
        {

            m_CurrentRound = m_RoundsList[round];
            m_HighestRound = m_CurrentRound;

            for (loop = 0; loop < m_CurrentRound.m_Loops.Length; loop++)
            {
                for (loopSequence = 0; loopSequence < m_CurrentRound.m_Loops[loop].m_LoopSequences.Length; loopSequence++)
                {
                    m_CurrentLoopSequence = m_CurrentRound.m_Loops[loop].m_LoopSequences[loopSequence];

                    if (m_CurrentLoopSequence.m_SequenceLength >= m_MaxSequenceLength)
                        m_MaxSequenceLength = m_CurrentLoopSequence.m_SequenceLength;

                    m_CurrentSequence = BuildSequence(m_CurrentLoopSequence.m_SequenceLength);
                    PlaySequence(m_CurrentSequence, m_CurrentLoopSequence);
                    PlayChronometerCoroutine();

                    yield return new WaitUntil(HasCurrentSequenceFinished);

                    m_CurrentTries++;
                    m_CurrentSuccessfulTries++;

                    m_TotalTries++;
                    m_TotalSuccessfulTries++;

                    m_CurrentSequenceIndex = 0;
                }

                while (CalculateSuccessRate(m_CurrentSuccessfulTries, m_CurrentTries) <= m_SuccessRateThreshold)
                {
                    m_CurrentSequence = BuildSequence(m_CurrentLoopSequence.m_SequenceLength);

                    PlaySequence(m_CurrentSequence, m_CurrentLoopSequence);
                    PlayChronometerCoroutine();

                    yield return new WaitUntil(HasCurrentSequenceFinished);

                    m_CurrentTries++;
                    m_CurrentSuccessfulTries++;

                    m_TotalTries++;
                    m_TotalSuccessfulTries++;

                    m_CurrentSequenceIndex = 0;
                }

                m_LevelTries.Add(m_CurrentTries);
                m_LevelSuccessfulTries.Add(m_CurrentSuccessfulTries);
                m_LevelSuccessRate.Add(CalculateSuccessRate(m_CurrentSuccessfulTries, m_CurrentTries));
                m_CurrentLevel++;

                m_CurrentTries = 0;
                m_CurrentSuccessfulTries = 0;
            }
        }

        int finalDifficultyIncrease = 0;
        m_CurrentLoopSequence = m_FinalLoopSequence;
        m_CurrentTries = 0;
        m_CurrentSuccessfulTries = 0;

        while (m_RemainingTime >= 0f)
        {
            m_CurrentSequence = BuildSequence(m_CurrentLoopSequence.m_SequenceLength);
            PlaySequence(m_CurrentSequence, m_CurrentLoopSequence);

            yield return new WaitUntil(HasCurrentSequenceFinished);

            m_CurrentTries++;
            m_CurrentSuccessfulTries++;
            m_CurrentLevel++;
            m_CurrentSequenceIndex = 0;
            finalDifficultyIncrease++;

            m_CurrentLoopSequence = new LoopSequence(m_FinalLoopSequence.m_SequenceLength + finalDifficultyIncrease,
                                                     m_FinalLoopSequence.m_RepeatsSequence,
                                                     m_FinalLoopSequence.m_RepetitionTime);

            if (m_CurrentLoopSequence.m_SequenceLength >= m_MaxSequenceLength)
                m_MaxSequenceLength = m_CurrentLoopSequence.m_SequenceLength;
        }
    }


    IEnumerator TimerCoroutine()
    {
        while (m_RemainingTime >= 0f)
        {
            m_RemainingTime -= Time.deltaTime;
            yield return null;
        }

        m_LevelTries.Add(m_CurrentTries);
        m_LevelSuccessfulTries.Add(m_CurrentSuccessfulTries);
        m_LevelSuccessRate.Add(CalculateSuccessRate(m_CurrentSuccessfulTries, m_CurrentTries));

        SaveManager.instance.SaveData();
        Debug.Log("Timer out");
    }

    void PlayChronometerCoroutine()
    {
        if (m_CurrentChronometerCoroutine != null)
        {
            StopCoroutine(m_CurrentChronometerCoroutine);
        }
        m_CurrentChronometerCoroutine = StartCoroutine(ChronometerCoroutine());
    }

    void StopChronometerCoroutine()
    {
        if (m_CurrentChronometerCoroutine != null)
        {
            StopCoroutine(m_CurrentChronometerCoroutine);
            m_SoundReactionTimes.Add(m_ChronometerTime);
        }
    }

    IEnumerator ChronometerCoroutine()
    {
        m_ChronometerTime = 0f;
        while (true)
        {
            m_ChronometerTime += Time.deltaTime;
            yield return null;
        }
    }

    bool HasCurrentSequenceFinished()
    {
        return (m_CurrentSequenceIndex >= m_CurrentSequence.Count);
    }
}
