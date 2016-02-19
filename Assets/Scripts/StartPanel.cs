using UnityEngine;

public class StartPanel : MonoBehaviour
{
    public GameObject gamePanel;

    public void StartButtonClick()
    {
        gameObject.SetActive(false);
        gamePanel.SetActive(true);
    }
}
