using UnityEngine;
using TMPro;
using System.Collections;

public class OldPhoto : MonoBehaviour
{
    [Header("Texto da Foto")]
    [TextArea(3, 6)]
    public string photoText;

    [Header("UI")]
    public GameObject investigationPanel;
    public TMP_Text investigationText;
    public GameObject collectHint;

    private bool dialogueOpen = false;
    private bool examined = false;
    private bool collected = false;
    private bool canClose = false;

    private void OnMouseEnter()
    {
        if (PauseHelper.BlockInput())
            return;

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
        if (PauseHelper.BlockInput())
            return;

        // Se o diálogo estiver aberto, não faz nada aqui.
        if (collected)
            return;

        // Abrir diálogo
        if (!dialogueOpen)
        {
            dialogueOpen = true;

            investigationPanel.SetActive(true);
            investigationText.gameObject.SetActive(true);

            // Usa o texto escrito no Inspector
            investigationText.text = photoText;
            investigationText.color = Color.white;
            investigationText.fontSize = 20;

            collectHint.SetActive(false);

            StartCoroutine(EnableClose());
        }
    }

    private IEnumerator EnableClose()
    {
        yield return null; // espera um frame
        canClose = true;
    }

    private void Update()
    {
        if (PauseHelper.BlockInput())
            return;

        // Fecha com clique ou espaço,
        // mas somente depois que o painel já apareceu.
        if (dialogueOpen &&
            canClose &&
            (Input.GetMouseButtonDown(0) ||
             Input.GetKeyDown(KeyCode.Space)))
        {
            dialogueOpen = false;
            examined = true;
            canClose = false;

            investigationPanel.SetActive(false);
            collectHint.SetActive(true);
        }

        // Guardar a fotografia
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