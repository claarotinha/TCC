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

    [Header("Áudio")]
    public AudioSource alarmSound;
    public AudioSource schoolAmbient;

    private int currentDialogue = 0;

    private void Start()
    {
        // O painel começa desativado
        dialoguePanel.SetActive(false);

        // Garante que o Fade começa transparente
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        // Inicia a sequência
        StartCoroutine(PlaySchoolScene());
    }

    IEnumerator PlaySchoolScene()
    {
        // =====================================================
        // ALARME ESCOLAR
        // =====================================================

        alarmSound.Play();

        // Espera o alarme terminar
        yield return new WaitForSeconds(alarmSound.clip.length);

        // =====================================================
        // SOM AMBIENTE
        // =====================================================

        schoolAmbient.Play();

        // =====================================================
        // PROFESSORA
        // =====================================================

        dialoguePanel.SetActive(true);

        portraitImage.sprite = professoraPortrait;
        nameText.text = "Professora";

        currentDialogue = 0;

        ShowProfessorDialogue();

        // Espera o jogador avançar pelas falas
        while (currentDialogue < 3)
        {
            yield return null;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                currentDialogue++;

                if (currentDialogue < 3)
                {
                    ShowProfessorDialogue();
                }
            }
        }

        // =====================================================
        // MARI
        // =====================================================

        portraitImage.sprite = mariPortrait;
        nameText.text = "Mari (pensamento)";

        dialogueText.text =
            "Ainda bem que a professora explicou a atividade novamente. " +
            "É melhor eu ir para casa e procurar algumas fotos da minha família.";

        // =====================================================
        // ESPERA O JOGADOR AVANÇAR
        // =====================================================

        while (true)
        {
            yield return null;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                break;
            }
        }

        // =====================================================
        // FINAL DA CENA
        // =====================================================

        yield return StartCoroutine(FadeAndLoadScene());
    }

    void ShowProfessorDialogue()
    {
        if (currentDialogue == 0)
        {
            dialogueText.text =
                "... Bem, turma, nossa aula chegou ao fim.";
        }
        else if (currentDialogue == 1)
        {
            dialogueText.text =
                "Só reforçando o que foi dito anteriormente: " +
                "o trabalho de história vai valer como nota da prova.";
        }
        else if (currentDialogue == 2)
        {
            dialogueText.text =
                "Vocês precisam pesquisar sobre a árvore genealógica da sua família. " +
                "É para a sexta, então recomendo que quem ainda não começou, comece imediatamente! " +
                "Estão dispensados, até amanhã!";
        }
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

        SceneManager.LoadScene("CaminhoParaCasa");
    }
}