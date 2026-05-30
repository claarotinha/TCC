using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Apertou E");

            currentInteractable?.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entrou em: " + other.name);

        IInteractable interactable =
            other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            Debug.Log("Objeto interativo encontrado");

            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable =
            other.GetComponent<IInteractable>();

        if (interactable == currentInteractable)
        {
            currentInteractable = null;
        }
    }
}