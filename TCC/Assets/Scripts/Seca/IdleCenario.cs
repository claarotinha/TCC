using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class IdleCenario : MonoBehaviour
{
    [SerializeField] private Sprite[] quadros;
    [SerializeField, Min(.02f)] private float intervalo = .25f;
    private SpriteRenderer desenho;
    private float tempo;

    private void Awake() => desenho = GetComponent<SpriteRenderer>();

    private void LateUpdate()
    {
        if (PauseHelper.BlockInput() || Time.timeScale <= 0f || quadros == null || quadros.Length == 0) return;
        tempo += Time.deltaTime;
        Sprite quadro = quadros[Mathf.FloorToInt(tempo / Mathf.Max(.02f, intervalo)) % quadros.Length];
        if (quadro != null) desenho.sprite = quadro;
    }
}
