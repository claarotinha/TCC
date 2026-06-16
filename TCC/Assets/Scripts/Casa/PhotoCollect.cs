using UnityEngine;
using TMPro;

public class PhotoCollect : MonoBehaviour
{
    public static int photosCollected = 0;

    [Header("Identificação")]
    public int photoID;

    [Header("UI")]
    public GameObject infoPanel;
    public TMP_Text infoText;

    private bool examined = false;
    private bool collected = false;
    private bool dialogueOpen = false;

    void Update()
    {
        // Fecha a informação
        if (dialogueOpen &&
            (Input.GetKeyDown(KeyCode.Space) ||
             Input.GetMouseButtonDown(0)))
        {
            CloseDialogue();
        }

        // Coleta
        if (examined &&
            !collected &&
            Input.GetKeyDown(KeyCode.E))
        {
            CollectPhoto();
        }
    }

    private void OnMouseEnter()
    {
        if (!collected && CursorManager.Instance != null)
        {
            CursorManager.Instance.SetLupa();
        }
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }
    }

    private void OnMouseDown()
    {
        if (collected || dialogueOpen)
            return;

        ShowDialogue();
    }

    void ShowDialogue()
    {
        dialogueOpen = true;

        infoPanel.SetActive(true);

        switch (photoID)
        {
            case 1:
                infoText.text =
                    "Uma foto de família.\n\nNão lembro de termos tirado tantas fotos assim.";
                break;

            case 2:
                infoText.text =
                    "Essa casa parece diferente.\n\nSerá que foi tirada há muito tempo?";
                break;

            case 3:
                infoText.text =
                    "Duas meninas...\n\nElas são tão parecidas.\n\nSerá que eram irmãs?";
                break;
        }
    }

    void CloseDialogue()
    {
        dialogueOpen = false;
        examined = true;

        infoPanel.SetActive(false);

        infoText.text =
            "Pressione E para guardar esta fotografia.";
    }

    void CollectPhoto()
    {
        collected = true;

        photosCollected++;

        Debug.Log("Fotos coletadas: " + photosCollected);

        if (photosCollected >= 3)
        {
            Debug.Log(
                "Mari: Essas fotos devem ajudar no trabalho... Mas acho que ainda não encontrei tudo o que preciso."
            );
        }

        gameObject.SetActive(false);

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }
    }
}