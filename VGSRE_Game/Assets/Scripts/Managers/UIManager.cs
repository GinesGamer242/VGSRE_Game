using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI m_CurrentDifficultyText;
    public TextMeshProUGUI m_CurrentRoundText;
    public RawImage m_CurrentSourceSprite;

    private void Update()
    {
        m_CurrentDifficultyText.text = GameManager.instance.m_CurrentDifficulty.ToString();
        m_CurrentRoundText.text = GameManager.instance.m_CurrentRound.ToString();

        if (GameManager.instance.m_CurrentSource)
        {
            m_CurrentSourceSprite.texture = GameManager.instance.m_CurrentSource.GetComponent<SpriteRenderer>().sprite.texture;
            m_CurrentSourceSprite.color = GameManager.instance.m_CurrentSource.GetComponent<SpriteRenderer>().color;
        }
    }
}
