using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject painelCreditos;
    public GameObject painelConfiguracoes;

    public void AbrirCreditos()
    {
        AudioManager.PlayItem("botao", 0.55f);
        painelCreditos.SetActive(true);
    }

    public void FecharCreditos()
    {
        AudioManager.PlayItem("botao", 0.55f);
        painelCreditos.SetActive(false);
    }

    public void AbrirConfiguracoes()
    {
        AudioManager.PlayItem("botao", 0.55f);
        painelConfiguracoes.SetActive(true);
    }

    public void FecharConfiguracoes()
    {
        AudioManager.PlayItem("botao", 0.55f);
        painelConfiguracoes.SetActive(false);
    }

    public void SairJogo()
    {
        Application.Quit();
    }
}