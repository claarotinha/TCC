using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuartinhoInvestigationSetup : MonoBehaviour
{
    [SerializeField] private GameObject[] investigationObjects;
    [SerializeField] private string[] descriptions;
    [SerializeField] private Texture2D magnifierCursor;
    [SerializeField] private BoxCollider2D roomBounds;

    private void Awake()
    {
        Camera camera = Camera.main;
        if (camera != null && roomBounds != null)
        {
            roomBounds.size = new Vector2(
                camera.orthographicSize * camera.aspect * 2f,
                roomBounds.size.y);
            roomBounds.transform.position = new Vector3(
                camera.transform.position.x,
                roomBounds.transform.position.y,
                0f);
        }

        CursorManager cursor = gameObject.AddComponent<CursorManager>();
        cursor.lupaCursor = magnifierCursor;

        GameObject panel = new GameObject("ExaminePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        RectTransform panelRect = (RectTransform)panel.transform;
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 24f);
        panelRect.sizeDelta = new Vector2(720f, 110f);
        panel.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.08f, 0.92f);

        GameObject textObject = new GameObject("ExamineText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel.transform, false);
        RectTransform textRect = (RectTransform)textObject.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 12f);
        textRect.offsetMax = new Vector2(-24f, -12f);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 24f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 16f;
        text.fontSizeMax = 24f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;

        for (int i = 0; i < investigationObjects.Length; i++)
        {
            GameObject item = investigationObjects[i];
            if (item == null || i >= descriptions.Length)
                continue;

            SpriteRenderer sprite = item.GetComponent<SpriteRenderer>();
            if (sprite == null || sprite.sprite == null)
                continue;

            BoxCollider2D collider = item.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = item.AddComponent<BoxCollider2D>();
            collider.size = sprite.sprite.bounds.size;
            collider.isTrigger = true;

            ExamineObject examine = item.GetComponent<ExamineObject>();
            if (examine == null)
                examine = item.AddComponent<ExamineObject>();
            examine.examinePanel = panel;
            examine.examineText = text;
            examine.message = descriptions[i];
        }

        panel.SetActive(false);
    }
}
