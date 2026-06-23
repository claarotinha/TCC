using UnityEngine;

public class UIParallax : MonoBehaviour
{
    [SerializeField] private float offsetMultiplier = 20f;
    [SerializeField] private float smoothTime = 0.3f;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 velocity;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        Vector2 mousePosition = Input.mousePosition;

        float x = (mousePosition.x / Screen.width) - 0.5f;
        float y = (mousePosition.y / Screen.height) - 0.5f;

        Vector2 targetPosition = startPosition + new Vector2(x, y) * offsetMultiplier;

        rectTransform.anchoredPosition = Vector2.SmoothDamp(
            rectTransform.anchoredPosition,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}