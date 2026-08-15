using UnityEngine;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("Intro");
        }
        else
        {
            Debug.LogError("GameManager não encontrado!");
        }
    }
}