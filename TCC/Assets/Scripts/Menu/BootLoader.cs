using UnityEngine;

public class BootLoader : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.LoadScene("Splash");
    }
}