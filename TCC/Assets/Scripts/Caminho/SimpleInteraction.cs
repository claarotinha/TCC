using UnityEngine;
using TMPro;

public class SimpleInteraction : MonoBehaviour
{
    public GameObject interactionText;

    private bool playerNearby = false;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            interactionText.SetActive(true);

            interactionText.GetComponent<TMP_Text>().text =
                "EITAA ENCONTREI UMA ARVORE RSRS";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        playerNearby = false;

        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }
}
}