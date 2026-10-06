using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Novo jogo inicia uma sessão limpa, mantendo o arquivo salvo até o próximo checkpoint.
        GameProgress.Instance?.ResetProgress();
        MotherDialogue.ResetSession();
        QuartoBaguncaDoor.ResetSession();
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ReplaceItems(null);
        if (InventoryTabController.Instance != null)
            InventoryTabController.Instance.Close();
        Time.timeScale = 1f;
        GameManager.Instance.LoadScene("AvisoInicial");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
