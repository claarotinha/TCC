using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.LoadScene("Escola");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}