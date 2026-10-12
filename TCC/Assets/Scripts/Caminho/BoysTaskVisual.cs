using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoysTaskVisual : MonoBehaviour
{
    [SerializeField] private Sprite[] noticeFrames;
    [SerializeField] private Sprite[] happyFrames;
    [SerializeField] private float framesPerSecond = 7.5f;

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
        marker.transform.localScale = Vector3.one * 0.35f;
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

        // Completed só acontece depois de devolver a bola e fechar o agradecimento.
        bool completed = tutorial.IsCompleted;
        if (completed && !happy)
        {
            happy = true;
            elapsed = 0f;
            if (originalAnimation != null)
                originalAnimation.enabled = false;
        }

        if (happy && happyFrames != null && happyFrames.Length > 0)
            boys.sprite = happyFrames[Frame(happyFrames.Length)];

        bool showNotice = !completed &&
            tutorial.CurrentStep >= TutorialManager.TutorialStep.Boys &&
            noticeFrames != null && noticeFrames.Length > 0;

        notice.gameObject.SetActive(showNotice);
        if (!showNotice) return;

        notice.sprite = noticeFrames[Frame(noticeFrames.Length)];
        Bounds bounds = boys.bounds;
        notice.transform.position = new Vector3(
            bounds.center.x,
            bounds.max.y + 0.2f + notice.bounds.extents.y,
            transform.position.z);
    }

    private int Frame(int count) =>
        Mathf.FloorToInt(elapsed * framesPerSecond) % count;
}
