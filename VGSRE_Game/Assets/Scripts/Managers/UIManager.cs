using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI m_CurrentSequenceText;
    public TextMeshProUGUI m_CurrentRoundText;
    public TextMeshProUGUI m_CurrentTriesText;
    public TextMeshProUGUI m_TimerText;
    public RawImage m_CurrentSourceSprite;

    private void Update()
    {
        m_CurrentRoundText.text = "";
        foreach (GameObject source in GameManager.instance.m_CurrentSequence)
        {
            m_CurrentRoundText.text += ($"{source.name}, ");
        }

        m_CurrentRoundText.text = GameManager.instance.m_CurrentRound.m_Number.ToString();

        m_CurrentTriesText.text = ($"Total: {GameManager.instance.m_CurrentTries}, successful: {GameManager.instance.m_CurrentSuccessfulTries}");

        m_TimerText.text = GameManager.instance.m_RemainingTime.ToString();

        if (GameManager.instance.m_CurrentSequence[GameManager.instance.m_CurrentSequenceIndex])
        {
            m_CurrentSourceSprite.texture = GameManager.instance.m_CurrentSequence[GameManager.instance.m_CurrentSequenceIndex].GetComponent<SpriteRenderer>().sprite.texture;
            m_CurrentSourceSprite.color = GameManager.instance.m_CurrentSequence[GameManager.instance.m_CurrentSequenceIndex].GetComponent<SpriteRenderer>().color;
        }
    }
}
