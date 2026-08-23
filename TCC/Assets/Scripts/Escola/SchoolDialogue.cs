using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SchoolDialogue : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Portraits")]
    public Sprite professoraPortrait;
    public Sprite mariPortrait;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Timing")]
    public float initialDelay = 10f;
    public float dialogueDuration = 5f;

    private void Start()
    {
        // O painel começa desativado
        dialoguePanel.SetActive(false);

        // Garante que o Fade começa transparente
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        // Inicia a sequência automaticamente
        StartCoroutine(PlaySchoolScene());
    }

    IEnumerator PlaySchoolScene()
    {
        // =====================================================
        // ESPERA INICIAL DE 10 SEGUNDOS
        // =====================================================

        yield return new WaitForSeconds(initialDelay);

        // =====================================================
        // ATIVA O PAINEL E MOSTRA A FALA DA PROFESSORA
        // =====================================================

        dialoguePanel.SetActive(true);

        portraitImage.sprite = professoraPortrait;
        nameText.text = "Professora";

        dialogueText.text =
            "... Bem, turma, nossa aula chegou ao fim, mas só reforçando " +
            "o que estava sendo dito anteriormente, o trabalho de história " +
            "vai valer como nota da prova. Vocês precisam pesquisar sobre a " +
            "árvore genealógica da sua família. É para a sexta, então recomendo " +
            "que, quem ainda não começou, comece imediatamente! Estão dispensados, " +
            "até amanhã!";

        // Espera a professora terminar
        yield return new WaitForSeconds(dialogueDuration);

        // =====================================================
        // PENSAMENTO DA MARI
        // =====================================================

        portraitImage.sprite = mariPortrait;
        nameText.text = "Mari (pensamento)";

        dialogueText.text =
            "Droga! Dormi a aula inteira. Ainda bem que a professora explicou " +
            "a atividade novamente. É melhor eu ir para casa.";

        // Espera o pensamento terminar
        yield return new WaitForSeconds(dialogueDuration);

        // =====================================================
        // FINAL DA CENA
        // =====================================================

        yield return StartCoroutine(FadeAndLoadScene());
    }

    IEnumerator FadeAndLoadScene()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            Color color = fadeImage.color;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);

            fadeImage.color = color;

            yield return null;
        }

        // Vai para a cena do caminho para casa
        SceneManager.LoadScene("CaminhoParaCasa");
    }
}