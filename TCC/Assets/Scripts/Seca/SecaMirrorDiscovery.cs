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
    [SerializeField] private GameObject reflectionBackdrop;

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
        if (reflectionImage != null) SetReflectionVisible(false);
    }

    private void OnMirrorOpened()
    {
        if (reflectionImage != null)
        {
            SetReflectionVisible(true);
        }
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
        if (!mirrorOpen) SetReflectionVisible(false);
    }

    private void OnDisable()
    {
        if (reflectionImage != null) SetReflectionVisible(false);
    }

    private void SetReflectionVisible(bool visible)
    {
        // Nunca desativa o pai inferido da imagem: ele pode ser o Canvas
        // compartilhado se a referência/hierarquia for editada no Inspector.
        if (reflectionBackdrop != null) reflectionBackdrop.SetActive(visible);
        if (reflectionImage != null) reflectionImage.gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        if (examineObject != null)
            examineObject.Opened -= OnMirrorOpened;
    }
}