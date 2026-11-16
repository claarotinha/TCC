using UnityEngine;

// A arte, escala e posição pertencem ao objeto filho editável na Hierarchy.
public class MirrorNotice : MonoBehaviour
{
    [SerializeField] private SecaMirrorDiscovery mirror;
    [SerializeField] private SpriteRenderer notice;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 7.5f;
    private float elapsed;

    private void LateUpdate()
    {
        if (mirror == null || notice == null) return;
        notice.gameObject.SetActive(!mirror.Discovered);
        if (mirror.Discovered || frames == null || frames.Length == 0) return;
        if (!PauseHelper.BlockInput()) elapsed += Time.deltaTime;
        notice.sprite = frames[Mathf.FloorToInt(elapsed * framesPerSecond) % frames.Length];
    }
}
