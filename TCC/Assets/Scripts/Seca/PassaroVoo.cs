using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PassaroVoo : MonoBehaviour
{
    [SerializeField] private Sprite[] quadros;
    [SerializeField, Min(0.02f)] private float intervalo = 0.2f;
    private SpriteRenderer desenho;
    private float tempo;
    private void Awake() => desenho = GetComponent<SpriteRenderer>();
    private void Update()
    {
        // A caça desativa o renderer; a animação não deve fazê-lo reaparecer.
        if (!desenho.enabled || PauseHelper.BlockInput() || Time.timeScale == 0f || quadros == null || quadros.Length == 0) return;
        tempo += Time.deltaTime;
        desenho.sprite = quadros[Mathf.FloorToInt(tempo / Mathf.Max(0.02f, intervalo)) % quadros.Length];
    }
}
