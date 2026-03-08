using UnityEngine;

public class MostrarElemento : MonoBehaviour
{
    public GameObject elementoParaMostrar;

    public void Ativar()
    {
        elementoParaMostrar.SetActive(true);
    }

    public void Alternar()
    {
        elementoParaMostrar.SetActive(!elementoParaMostrar.activeSelf);
    }

    public void Desativar()
    {
        elementoParaMostrar.SetActive(false);
    }
}