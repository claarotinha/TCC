using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SchoolDialogue : MonoBehaviour
{
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    public Sprite professoraPortrait;
    public Sprite mariPortrait;

    public Image fadeImage;
    public float fadeDuration = 1f;

    private int currentDialogue = 0;

    void Start()
    {
        ShowDialogue();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            currentDialogue++;

            if (currentDialogue < 2)
            {
                ShowDialogue();
            }
            else
            {
                StartCoroutine(FadeAndLoadScene());
            }
        }
    }

    void ShowDialogue()
    {
        if (currentDialogue == 0)
        {
            portraitImage.sprite = professoraPortrait;
            nameText.text = "Professora";
            dialogueText.text = "Pessoal, o próximo trabalho será sobre a história das suas famílias.";
        }
        else if (currentDialogue == 1)
        {
            portraitImage.sprite = mariPortrait;
            nameText.text = "Mari";
            dialogueText.text = "Árvore genealógica...? Acho que vou precisar procurar algumas fotos em casa.";
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