using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.LoadScene("AvisoInicial");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}