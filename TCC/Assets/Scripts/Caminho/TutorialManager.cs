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

    public TutorialStep CurrentStep { get; private set; }
    public bool IsCompleted => CurrentStep == TutorialStep.Completed;

    private float lastPlayerX;
    private float distanceInStep;

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

    public void ReportInvestigation()
    {
        if (CurrentStep == TutorialStep.Investigate)
            SetStep(TutorialStep.CryingBoy);
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
                tutorialText.text = "Clique em um objeto do cenário para investigar.";
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
                tutorialText.text = "Os garotos agradeceram. Agora você pode entrar em casa.";
                break;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}