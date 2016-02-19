using UnityEngine;
using UnityEngine.UI;

public class PatternPanel : MonoBehaviour
{
    public Image patternImage;

    private void OnEnable()
    {
        DrawPanel.PatternChanged -= DrawPanelOnPatternChanged;
        DrawPanel.PatternChanged += DrawPanelOnPatternChanged;
    }

    void OnDisable()
    {
        DrawPanel.PatternChanged -= DrawPanelOnPatternChanged;
    }

    private void DrawPanelOnPatternChanged(Sprite sprite)
    {
        patternImage.sprite = sprite;
    }
}
