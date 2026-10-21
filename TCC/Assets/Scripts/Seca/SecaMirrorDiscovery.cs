using UnityEngine;

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
    }

    private void OnMirrorOpened()
    {
        if (Discovered)
            return;

        Discovered = true;

        // A primeira mensagem já está na tela.
        // Esta será usada somente quando o espelho for aberto novamente.
        examineObject.message = repeatMessage;
    }

    private void OnDestroy()
    {
        if (examineObject != null)
            examineObject.Opened -= OnMirrorOpened;
    }
}