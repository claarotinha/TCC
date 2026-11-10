using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PassaroVoo : MonoBehaviour
{
    [SerializeField] private Sprite[] quadros;
    [SerializeField, Min(0.02f)] private float intervalo = 0.2f;
    [SerializeField, Min(0f)] private float alcanceHorizontal = 2.5f;
    [SerializeField, Min(0f)] private float alturaVoo = 0.35f;
    [SerializeField, Min(0.1f)] private float velocidadeVoo = 0.8f;
    private Vector3 origem;
    private SpriteRenderer desenho;
    private float tempo;
    private void Awake()
    {
        desenho = GetComponent<SpriteRenderer>();
        origem = transform.localPosition;
    }
    private void Update()
    {
        // A caça desativa o renderer; a animação não deve fazê-lo reaparecer.
        if (!desenho.enabled || PauseHelper.BlockInput() || Time.timeScale == 0f || quadros == null || quadros.Length == 0) return;
        tempo += Time.deltaTime;
        Vector3 antes = transform.localPosition;
        transform.localPosition = origem + new Vector3(
            Mathf.Sin(tempo * velocidadeVoo) * alcanceHorizontal,
            Mathf.Sin(tempo * velocidadeVoo * 2f) * alturaVoo, 0f);
        float dx = transform.localPosition.x - antes.x;
        if (Mathf.Abs(dx) > 0.00001f) desenho.flipX = dx < 0f;
        desenho.sprite = quadros[Mathf.FloorToInt(tempo / Mathf.Max(0.02f, intervalo)) % quadros.Length];
    }
}
