using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public TMP_Text tutorialText;

    private bool moved = false;
    private bool ran = false;

    void Update()
    {
        if (!moved && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            moved = true;
            tutorialText.text = "Segure Shift para correr.";
        }

        else if (moved && !ran && Input.GetKey(KeyCode.LeftShift))
        {
            ran = true;
            tutorialText.text = "Pressione E para interagir.";
        }

        else if (ran && Input.GetKeyDown(KeyCode.E))
        {
            tutorialText.gameObject.SetActive(false);
        }
    }
}