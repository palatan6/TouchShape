using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text seconds;
    public Text miliSeconds;

    public float startRoundTime = 1500;
    public float nextRoundChip = 100;
    private float _currentRoundTime;

    private int _sec;
    private int _miliSec;

    public float eps = 0.01f;

    private IEnumerator _counter;

    public GameObject gameOver;

    public void CountBack(float miliseconds)
    {
        if (_counter!=null)
        {
            StopCoroutine(_counter);
        }

        _counter = CountBackRoutine(miliseconds);

        StartCoroutine(_counter);
    }

    IEnumerator CountBackRoutine(float miliseconds)
    {
        float counter = miliseconds;

        while (counter > eps)
        {
            yield return new WaitForFixedUpdate();

            _sec = (int)counter / 100;
            seconds.text = _sec.ToString();

            _miliSec = (int)(counter - _sec*100);

            if (_miliSec < 10)
                miliSeconds.text = "0" + _miliSec;
            else
                miliSeconds.text = _miliSec.ToString();

            counter -= Time.fixedDeltaTime*100;
        }

        GameOver();
    }

    void Start()
    {
        _currentRoundTime = startRoundTime;

        CountBack(_currentRoundTime);
    }

    void OnEnable()
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
        _currentRoundTime -= nextRoundChip;
        CountBack(_currentRoundTime);
    }

    void GameOver()
    {
        gameOver.SetActive(true);
    }
}
