using UnityEngine;

// Acompanha também movimentos de cutscene, quando o seguidor está desativado.
[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AnimacaoPersonagem : MonoBehaviour
{
    private static readonly int Andando = Animator.StringToHash("IsWalking");
    private static readonly int Correndo = Animator.StringToHash("IsRunning");
    private Animator animador;
    private SpriteRenderer desenho;
    private Vector3 posicaoAnterior;

    private void Awake()
    {
        animador = GetComponent<Animator>();
        desenho = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() => posicaoAnterior = transform.position;

    private void LateUpdate()
    {
        float deslocamento = transform.position.x - posicaoAnterior.x;
        posicaoAnterior = transform.position;
        // Ferimentos e sequências de memória desativam o Animator de propósito.
        if (animador == null || !animador.enabled) return;

        bool andando = Time.timeScale > 0f && Mathf.Abs(deslocamento) > 0.001f;
        bool correndo = andando && Mathf.Abs(deslocamento) / Mathf.Max(Time.deltaTime, 0.0001f) > 6.5f;
        animador.SetBool(Andando, andando);
        animador.SetBool(Correndo, correndo);
        if (andando)
            desenho.flipX = deslocamento * transform.lossyScale.x < 0f;
    }
}
