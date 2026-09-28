using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public enum TutorialStep
    {
        Move,
        Run,
        Investigate,
        CryingBoy,
        Boys,
        FindBall,
        OpenInventory,
        ReturnBall,
        Completed
    }

    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Transform player;
    [SerializeField] private float requiredDistance = 0.5f;
    [SerializeField] private GameObject[] investigationTargets;

    public TutorialStep CurrentStep { get; private set; }
    public bool IsCompleted => CurrentStep == TutorialStep.Completed;

    private float lastPlayerX;
    private float distanceInStep;
    private readonly HashSet<GameObject> investigated = new HashSet<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject mari = GameObject.FindGameObjectWithTag("Player");
            if (mari != null)
                player = mari.transform;
        }

        if (player != null)
            lastPlayerX = player.position.x;

        if (investigationTargets == null || investigationTargets.Length != 5)
            Debug.LogError("Tutorial: configure os cinco objetos investigáveis no TutorialManager.", this);

        SetStep(TutorialStep.Move);
    }

    private void Update()
    {
        if (player == null || IsCompleted)
            return;

        float currentX = player.position.x;
        float distanceMoved = Mathf.Abs(currentX - lastPlayerX);
        lastPlayerX = currentX;

        if (UniversalPauseManager.IsPaused || NPCDialogue.IsShowing())
            return;

        bool walking = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0f;

        if (CurrentStep == TutorialStep.Move && walking)
        {
            distanceInStep += distanceMoved;

            if (distanceInStep >= requiredDistance)
                SetStep(TutorialStep.Run);
        }
        else if (CurrentStep == TutorialStep.Run &&
                 walking && Input.GetKey(KeyCode.LeftShift))
        {
            distanceInStep += distanceMoved;

            if (distanceInStep >= requiredDistance)
                SetStep(TutorialStep.Investigate);
        }
        else if (CurrentStep == TutorialStep.OpenInventory &&
                 InventoryTabController.Instance != null &&
                 InventoryTabController.Instance.IsOpen)
        {
            SetStep(TutorialStep.ReturnBall);
        }
    }

    public void ReportInvestigation(GameObject target)
    {
        if (CurrentStep != TutorialStep.Investigate || target == null)
            return;

        if (investigationTargets == null || System.Array.IndexOf(investigationTargets, target) < 0)
        {
            Debug.LogWarning("Tutorial: objeto fora da lista de investigação: " + target.name, target);
            return;
        }

        if (!investigated.Add(target))
        {
#if UNITY_EDITOR
            Debug.Log("Tutorial: " + target.name + " já foi investigado.", target);
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Tutorial: " + target.name + " investigado (" +
                  investigated.Count + "/" + investigationTargets.Length + ").", target);
#endif
        if (investigated.Count == investigationTargets.Length)
            SetStep(TutorialStep.CryingBoy);
        else
            ShowInvestigationProgress();
    }

    public void ReportCryingBoy()
    {
        if (CurrentStep == TutorialStep.CryingBoy)
            SetStep(TutorialStep.Boys);
    }

    public void ReportBoysClue()
    {
        if (CurrentStep == TutorialStep.Boys)
            SetStep(TutorialStep.FindBall);
    }

    public void ReportBallCollected()
    {
        if (CurrentStep == TutorialStep.FindBall)
            SetStep(TutorialStep.OpenInventory);
    }

    public void ReportBallReturned()
    {
        if (CurrentStep == TutorialStep.ReturnBall)
            SetStep(TutorialStep.Completed);
    }

    public void HideTutorial()
    {
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(false);
    }

    public void ShowTutorial()
    {
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(true);
    }

    private void SetStep(TutorialStep nextStep)
    {
        CurrentStep = nextStep;
        distanceInStep = 0f;

        if (tutorialText == null)
            return;

        tutorialText.gameObject.SetActive(true);

        switch (nextStep)
        {
            case TutorialStep.Move:
                tutorialText.text = "Use A e D para se movimentar.";
                break;
            case TutorialStep.Run:
                tutorialText.text = "Segure Shift enquanto anda para correr.";
                break;
            case TutorialStep.Investigate:
                ShowInvestigationProgress();
                break;
            case TutorialStep.CryingBoy:
                tutorialText.text = "Clique no garoto que está chorando.";
                break;
            case TutorialStep.Boys:
                tutorialText.text = "Converse com o grupo de garotos.";
                break;
            case TutorialStep.FindBall:
                tutorialText.text = "Procure a bola e escolha se deseja coletá-la.";
                break;
            case TutorialStep.OpenInventory:
                tutorialText.text = "Pressione TAB para abrir o inventário.";
                break;
            case TutorialStep.ReturnBall:
                tutorialText.text = "Arraste a bola do inventário até os garotos.";
                break;
            case TutorialStep.Completed:
                tutorialText.text = "Os garotos agradeceram. Clique na porta para entrar em casa.";
                break;
        }

#if UNITY_EDITOR
        Debug.Log("Tutorial: etapa atual = " + nextStep + ".", this);
#endif
    }

    private void ShowInvestigationProgress()
    {
        if (tutorialText != null)
            tutorialText.text = "Investigue árvore, bicicleta, padaria, casa e bola (" +
                                investigated.Count + "/5). Clique com o mouse.";
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
