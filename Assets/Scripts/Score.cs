using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    public Text scoreText;

    public static int scoreCount = 0;

    void Start()
    {
        scoreCount = 0;
        UpdateText();
    }

    private void OnEnable()
    {
        DrawPanel.RoundDone -= DrawPanelOnRoundDone;
        DrawPanel.RoundDone += DrawPanelOnRoundDone;
    }

    void OnDisable()
    {
        DrawPanel.RoundDone -= DrawPanelOnRoundDone;
    }

    private void DrawPanelOnRoundDone()
    {
        ++scoreCount;
        UpdateText();
    }

    void UpdateText()
    {
        scoreText.text = scoreCount.ToString();
    }
}
