using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject painelCreditos;
    public GameObject painelConfiguracoes;

    public void AbrirCreditos()
    {
        painelCreditos.SetActive(true);
    }

    public void FecharCreditos()
    {
        painelCreditos.SetActive(false);
    }

    public void AbrirConfiguracoes()
    {
        painelConfiguracoes.SetActive(true);
    }

    public void FecharConfiguracoes()
    {
        painelConfiguracoes.SetActive(false);
    }

    public void SairJogo()
    {
        Application.Quit();
    }
}