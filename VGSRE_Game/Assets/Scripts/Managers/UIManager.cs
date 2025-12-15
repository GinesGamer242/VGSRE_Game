using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI m_CurrentSequenceText;
    public TextMeshProUGUI m_CurrentRoundText;
    public TextMeshProUGUI m_CurrentTriesText;
    public TextMeshProUGUI m_TimerText;
    public RawImage m_CurrentSourceSprite;
    public TextMeshProUGUI m_FinalData;


    // Session completed
    public GameObject m_EndSessionPanel;

    private void Update()
    {
        m_CurrentRoundText.text = "";
        foreach (GameObject source in GameManager.instance.m_CurrentSequence)
        {
            m_CurrentRoundText.text += ($"{source.name}, ");
        }

        m_CurrentRoundText.text = $"Level: {(GameManager.instance.m_CurrentRound.m_Number + 1).ToString()}";

        m_CurrentTriesText.text = ($"Total: {GameManager.instance.m_TotalTries}, successful: {GameManager.instance.m_TotalSuccessfulTries}");

        m_TimerText.text = GameManager.instance.m_RemainingTime.ToString();

        if (GameManager.instance.m_CurrentSequence[GameManager.instance.m_CurrentSequenceIndex])
        {
            m_CurrentSourceSprite.texture = GameManager.instance.m_CurrentSequence[GameManager.instance.m_CurrentSequenceIndex].GetComponent<SpriteRenderer>().sprite.texture;
            m_CurrentSourceSprite.color = GameManager.instance.m_CurrentSequence[GameManager.instance.m_CurrentSequenceIndex].GetComponent<SpriteRenderer>().color;
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void SessionCompleted()
    {
        m_EndSessionPanel.SetActive(true);
        m_FinalData.text = $"Level Reached: {GameManager.instance.m_CurrentRound.m_Number + 1} " +
            $"\nSounds Found: {GameManager.instance.m_TotalSuccessfulSounds}" +
            $"\nTotal Successful Tries: {GameManager.instance.m_TotalSuccessfulTries} / {GameManager.instance.m_TotalTries}";
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }

}
