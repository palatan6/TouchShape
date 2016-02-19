using UnityEngine;
using System.Collections;

public class ResultText : MonoBehaviour
{
    public GameObject great;
    public GameObject wrong;

    public float showTime = 0.7f;

    private IEnumerator _showWord;

    void OnEnable()
    {
        DrawPanel.DrawFinished -= DrawPanelOnDrawFinished;
        DrawPanel.DrawFinished += DrawPanelOnDrawFinished;
    }

    void OnDisable()
    {
        DrawPanel.DrawFinished -= DrawPanelOnDrawFinished;
    }

    private void DrawPanelOnDrawFinished(bool val)
    {
        great.SetActive(false);
        wrong.SetActive(false);

        if (_showWord!=null)
        {
            StopCoroutine(_showWord);
        }

        _showWord = ShowWord(val ? great : wrong);

        StartCoroutine(_showWord);
    }

    IEnumerator ShowWord(GameObject word)
    {
        word.SetActive(true);
        yield return new WaitForSeconds(showTime);
        word.SetActive(false);
    }
}
