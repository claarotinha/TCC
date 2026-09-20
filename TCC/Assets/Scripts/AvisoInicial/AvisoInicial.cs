using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class AvisoInicial : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text textoAviso;

    [Header("Som")]
    public AudioSource typingSound;

    [Header("Configuração")]
    public float tempoEntreLetras = 0.08f;
    public float tempoDepoisDoTexto = 2f;

    private string mensagem = "Rio Grande do Norte, 1990...";

    private void Start()
    {
        textoAviso.text = "";

        StartCoroutine(DigitarTexto());
    }

    IEnumerator DigitarTexto()
    {
        typingSound.Play();

        foreach (char letra in mensagem)
        {
            textoAviso.text += letra;

            yield return new WaitForSeconds(tempoEntreLetras);
        }

        typingSound.Stop();

        yield return new WaitForSeconds(tempoDepoisDoTexto);

        SceneManager.LoadScene("Escola");
    }
}