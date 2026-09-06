using UnityEngine;
using TMPro;

public class SimpleInteraction : MonoBehaviour
{
    public GameObject interactionText;
    public string interactionMessage = "EITAA ENCONTREI UMA ARVORE RSRS";

    private bool playerNearby = false;
    private bool hasInteracted = false;

    void Update()
    {
        // SE O DIÁLOGO ESTIVER ABERTO, NÃO INTERAGE
        if (NPCDialogue.IsShowing())
            return;

        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !hasInteracted)
        {
            hasInteracted = true;
            
            if (interactionText != null)
            {
                interactionText.SetActive(true);
                TMP_Text text = interactionText.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.text = interactionMessage;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            hasInteracted = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            hasInteracted = false;

            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
        }
    }
}