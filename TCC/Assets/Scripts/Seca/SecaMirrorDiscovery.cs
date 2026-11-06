using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ExamineObject))]
public class SecaMirrorDiscovery : MonoBehaviour
{
    [Header("Primeira investigação")]
    [SerializeField, TextArea(3, 6)]
    private string discoveryMessage =
        "Essa… Essa não sou eu. É a bisa… " +
        "É o rosto da minha bisavó, Zefinha! " +
        "Meu Deus, o que está acontecendo?";

    [Header("Investigações seguintes")]
    [SerializeField, TextArea(3, 6)]
    private string repeatMessage =
        "O reflexo continua sendo o de Zefinha. " +
        "Não sei como isso é possível, mas preciso entender onde estou.";

    [Header("Imagem do reflexo") ]
    [SerializeField] private Image reflectionImage;

    public bool Discovered { get; private set; }

    private ExamineObject examineObject;

    private void Awake()
    {
        examineObject = GetComponent<ExamineObject>();
        examineObject.Opened += OnMirrorOpened;
    }

    private void Start()
    {
        examineObject.message = discoveryMessage;
        if (reflectionImage != null) reflectionImage.gameObject.SetActive(false);
    }

    private void OnMirrorOpened()
    {
        if (reflectionImage != null) reflectionImage.gameObject.SetActive(true);
        if (Discovered)
            return;

        Discovered = true;

        // A primeira mensagem já está na tela.
        // Esta será usada somente quando o espelho for aberto novamente.
        examineObject.message = repeatMessage;
    }

    private void LateUpdate()
    {
        if (reflectionImage == null || !reflectionImage.gameObject.activeSelf) return;
        bool mirrorOpen = examineObject.examinePanel != null &&
            examineObject.examinePanel.activeInHierarchy && examineObject.examineText != null &&
            (examineObject.examineText.text == discoveryMessage ||
             examineObject.examineText.text == repeatMessage);
        if (!mirrorOpen) reflectionImage.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (reflectionImage != null) reflectionImage.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (examineObject != null)
            examineObject.Opened -= OnMirrorOpened;
    }
}