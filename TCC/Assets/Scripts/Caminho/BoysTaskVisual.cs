using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoysTaskVisual : MonoBehaviour
{
    [SerializeField] private Sprite[] noticeFrames;
    [SerializeField] private Sprite[] happyFrames;
    [SerializeField] private float framesPerSecond = 7.5f;

    [SerializeField] private float noticeScale = 0.18f;
    [SerializeField] private float noticeGap = 0.03f;

    private SpriteRenderer boys;
    private Animator originalAnimation;
    private SpriteRenderer notice;
    private float elapsed;
    private bool happy;

    private void Awake()
    {
        boys = GetComponent<SpriteRenderer>();
        originalAnimation = GetComponent<Animator>();
        GameObject marker = new GameObject("AvisoTask");
        marker.transform.SetParent(transform, false);
        marker.transform.localScale = Vector3.one * noticeScale;
        notice = marker.AddComponent<SpriteRenderer>();
        notice.sharedMaterial = boys.sharedMaterial;
        notice.sortingLayerID = boys.sortingLayerID;
        notice.sortingOrder = boys.sortingOrder + 5;
        marker.SetActive(false);
    }

    private void LateUpdate()
    {
        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial == null)
        {
            notice.gameObject.SetActive(false);
            return;
        }

        if (!UniversalPauseManager.IsPaused)
            elapsed += Time.deltaTime;

        if (tutorial.IsCompleted && !happy)
            BallDelivered();

        if (happy && happyFrames != null && happyFrames.Length > 0)
            boys.sprite = happyFrames[Frame(happyFrames.Length)];

        bool showNotice = !happy &&
            tutorial.CurrentStep >= TutorialManager.TutorialStep.Boys &&
            noticeFrames != null && noticeFrames.Length > 0;

        notice.gameObject.SetActive(showNotice);
        if (!showNotice) return;

        notice.sprite = noticeFrames[Frame(noticeFrames.Length)];
        Bounds bounds = boys.bounds;
        notice.transform.position = new Vector3(
            bounds.center.x,
            bounds.max.y + noticeGap + notice.bounds.extents.y,
            transform.position.z);
    }

    public void BallDelivered()
    {
        if (happy) return;
        happy = true;
        elapsed = 0f;
        if (originalAnimation != null)
            originalAnimation.enabled = false;
        if (happyFrames != null && happyFrames.Length > 0)
            boys.sprite = happyFrames[0];
        notice.gameObject.SetActive(false);
    }

    private int Frame(int count) =>
        Mathf.FloorToInt(elapsed * framesPerSecond) % count;
}
