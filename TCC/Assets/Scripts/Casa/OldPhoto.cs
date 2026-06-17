using UnityEngine;
using TMPro;

public class OldPhoto : MonoBehaviour
{
    [Header("Texto da Foto")]
    [TextArea]
    public string photoText;

    [Header("UI")]
    public GameObject investigationPanel;
    public TMP_Text investigationText;
    public GameObject collectHint;

    private bool dialogueOpen = false;
    private bool examined = false;
    private bool collected = false;

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
        if (dialogueOpen || collected)
            return;

        dialogueOpen = true;

        investigationPanel.SetActive(true);

investigationText.gameObject.SetActive(true);

investigationText.text =
    "TESTE\nTESTE\nTESTE";

investigationText.color = Color.red;
investigationText.fontSize = 60;

Debug.Log("Texto atribuído.");
    }

    private void Update()
    {
        // Fechar a descrição
        if (dialogueOpen &&
            (Input.GetMouseButtonDown(0) ||
             Input.GetKeyDown(KeyCode.Space)))
        {
            dialogueOpen = false;
            examined = true;

            investigationPanel.SetActive(false);
            collectHint.SetActive(true);
        }

        // Guardar a foto
        if (examined &&
            !collected &&
            Input.GetKeyDown(KeyCode.E))
        {
            collected = true;

            collectHint.SetActive(false);

            gameObject.SetActive(false);

            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.SetNormal();
            }

            Debug.Log("Fotografia guardada.");
        }
    }
}