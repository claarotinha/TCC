using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotherDialogue : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;

    public Image portraitImage;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;

    [Header("Choices")]
    public Button choice1;
    public Button choice2;
    public Button choice3;

    [Header("Portrait")]
    public Sprite motherPortrait;

    private bool dialogueOpen = false;

    void OnMouseDown()
    {
        if (dialogueOpen)
            return;

        OpenChoices();
    }

    void OpenChoices()
    {
        dialogueOpen = true;

        dialoguePanel.SetActive(true);

        portraitImage.sprite = motherPortrait;

        characterNameText.text = "Mãe";

        dialogueText.text = "O que foi, Mari?";

        choice1.GetComponentInChildren<TMP_Text>().text = "Conversar";
        choice2.GetComponentInChildren<TMP_Text>().text = "Perguntar sobre o trabalho";
        choice3.GetComponentInChildren<TMP_Text>().text = "Sair";

        choice1.onClick.RemoveAllListeners();
        choice2.onClick.RemoveAllListeners();
        choice3.onClick.RemoveAllListeners();

        choice1.onClick.AddListener(Conversar);
        choice2.onClick.AddListener(PerguntarTrabalho);
        choice3.onClick.AddListener(FecharDialogo);
    }

    void Conversar()
    {
        dialogueText.text =
            "Como foi a escola hoje, filha? Você parece pensativa.";
    }

    void PerguntarTrabalho()
    {
        dialogueText.text =
            "Esse trabalho sobre a família pode ser uma boa oportunidade para conhecer mais sobre as nossas origens.";
    }

    void FecharDialogo()
    {
        dialoguePanel.SetActive(false);

        dialogueOpen = false;
    }
}