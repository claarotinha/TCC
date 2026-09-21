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
        dialoguePanel.SetActive(false);

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        StartCoroutine(PlaySchoolScene());
    }

    IEnumerator PlaySchoolScene()
    {
        // Espera 5 segundos antes de tocar o alarme
        yield return new WaitForSeconds(5f);

        if (alarmSound != null)
            alarmSound.Play();

        // Espera o alarme terminar
        if (alarmSound != null && alarmSound.clip != null)
            yield return new WaitForSeconds(alarmSound.clip.length);

        // Começa o som ambiente da escola
        if (schoolAmbient != null)
            schoolAmbient.Play();

        dialoguePanel.SetActive(true);

        portraitImage.sprite = professoraPortrait;
        nameText.text = "Professora";

        currentDialogue = 0;
        ShowProfessorDialogue();

        // Diálogos da professora
        while (currentDialogue < 3)
        {
            yield return null;

            // Avança com clique
            if (Input.GetMouseButtonDown(0))
            {
                currentDialogue++;

                if (currentDialogue < 3)
                    ShowProfessorDialogue();
            }
        }

        // Pensamento da Mari
        portraitImage.sprite = mariPortrait;
        nameText.text = "Mari (pensamento)";

        dialogueText.text =
            "Ainda bem que a professora explicou a atividade novamente. " +
            "É melhor eu ir para casa e procurar algumas fotos da minha família.";

        // Espera clique
        while (true)
        {
            yield return null;

            if (Input.GetMouseButtonDown(0))
                break;
        }

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