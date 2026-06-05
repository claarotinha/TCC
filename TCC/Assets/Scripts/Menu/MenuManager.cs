using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject painelCreditos;

    public void AbrirCreditos()
    {
        painelCreditos.SetActive(true);
    }

    public void FecharCreditos()
    {
        painelCreditos.SetActive(false);
    }
}