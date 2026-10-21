using UnityEngine;

[RequireComponent(typeof(ExamineObject))]
public class SecaRoomInvestigation : MonoBehaviour
{
    [Header("Liberação da investigação")]
    [SerializeField] private SecaSleepSequence sleepSequence;

    public bool Inspected { get; private set; }

    private ExamineObject examineObject;
    private bool investigationReleased;

    private void Awake()
    {
        examineObject = GetComponent<ExamineObject>();

        // Impede investigação e lupa antes de terminar a sequência de dormir.
        examineObject.enabled = false;
        examineObject.Opened += RegisterInvestigation;
    }

    private void Start()
    {
        if (sleepSequence == null)
        {
            Debug.LogError(
                "SecaRoomInvestigation: arraste SecaSono para Sleep Sequence em " +
                gameObject.name + ".",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (investigationReleased ||
            sleepSequence == null ||
            !sleepSequence.Finished)
        {
            return;
        }

        investigationReleased = true;
        examineObject.enabled = true;
    }

    private void RegisterInvestigation()
    {
        if (!investigationReleased)
            return;

        Inspected = true;
    }

    private void OnDestroy()
    {
        if (examineObject != null)
            examineObject.Opened -= RegisterInvestigation;
    }
}