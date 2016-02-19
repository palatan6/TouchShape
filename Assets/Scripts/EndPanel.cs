using UnityEngine;
using UnityEngine.UI;

public class EndPanel : MonoBehaviour
{
    public Text score;

    void OnEnable()
    {
        score.text = Score.scoreCount.ToString();
    }

    public void RestartButtonClick()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }
}
