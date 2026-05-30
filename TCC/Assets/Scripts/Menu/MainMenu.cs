using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.LoadScene("MENU");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
