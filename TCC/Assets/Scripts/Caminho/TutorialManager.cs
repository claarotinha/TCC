using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public TMP_Text tutorialText;

    private bool moved = false;
    private bool ran = false;
    private bool tutorialCompleted = false;

    void Update()
    {
        // SE O DIÁLOGO ESTIVER ABERTO, NÃO FAZ NADA
        if (NPCDialogue.IsShowing())
            return;

        if (tutorialCompleted)
            return;

        if (!moved && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            moved = true;
            if (tutorialText != null)
                tutorialText.text = "Segure Shift para correr.";
        }

        else if (moved && !ran && Input.GetKey(KeyCode.LeftShift))
        {
            ran = true;
            if (tutorialText != null)
                tutorialText.text = "Pressione E para interagir.";
        }

        else if (ran && Input.GetKeyDown(KeyCode.E))
        {
            tutorialCompleted = true;
            if (tutorialText != null)
            {
                tutorialText.text = "Tutorial concluído!";
                // NÃO DESATIVA - só muda o texto
                // tutorialText.gameObject.SetActive(false); // REMOVIDO
            }
        }
    }

    // Método para o diálogo esconder o tutorial
    public void HideTutorial()
    {
        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
        }
    }

    public void ShowTutorial()
    {
        if (tutorialText != null && !tutorialCompleted)
        {
            tutorialText.gameObject.SetActive(true);
        }
    }
}