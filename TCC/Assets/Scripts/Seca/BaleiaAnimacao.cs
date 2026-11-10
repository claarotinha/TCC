using UnityEngine;

[DefaultExecutionOrder(1100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class BaleiaAnimacao : MonoBehaviour
{
    [SerializeField] private Sprite parada;
    [SerializeField] private Sprite[] caminhada;
    [SerializeField] private Sprite[] corrida;
    [SerializeField, Min(0.02f)] private float intervaloCaminhada = 0.24f;
    [SerializeField, Min(0.02f)] private float intervaloCorrida = 0.13f;
    private SpriteRenderer desenho;
    private SeguidorFamilia seguidor;
    private Vector3 ultimaPosicao;
    private float tempo;
    private int estadoAnterior = -1;
    private bool poseExterna;

    private void Awake()
    {
        desenho = GetComponent<SpriteRenderer>();
        seguidor = GetComponent<SeguidorFamilia>();
        // Um único sistema escreve nos sprites, inclusive ao começar pelo Boot.
        Animator animador = GetComponent<Animator>();
        if (animador != null) animador.enabled = false;
        if (parada == null) parada = desenho.sprite;
    }

    private void OnEnable()
    {
        ultimaPosicao = transform.position;
        tempo = 0f;
        estadoAnterior = -1;
    }

    public void DefinirPoseExterna(bool ativa)
    {
        poseExterna = ativa;
        tempo = 0f;
        estadoAnterior = -1;
        ultimaPosicao = transform.position;
    }

    private void LateUpdate()
    {
        float deslocamento = transform.position.x - ultimaPosicao.x;
        ultimaPosicao = transform.position;
        if (poseExterna || PauseHelper.BlockInput() || Time.timeScale == 0f || !desenho.enabled) return;
        bool emFila = seguidor != null && seguidor.isActiveAndEnabled;
        bool andando = emFila ? seguidor.EstaAndando : Mathf.Abs(deslocamento) > 0.001f;
        bool correndo = emFila ? seguidor.EstaCorrendo
            : andando && Mathf.Abs(deslocamento) / Mathf.Max(Time.deltaTime, 0.0001f) > 6.5f;
        int estado = !andando ? 0 : correndo ? 2 : 1;
        if (estado != estadoAnterior) tempo = 0f;
        else tempo += Time.deltaTime;
        estadoAnterior = estado;
        if (!andando)
        {
            if (parada != null) desenho.sprite = parada;
            return;
        }
        Sprite[] quadros = correndo ? corrida : caminhada;
        if (quadros != null && quadros.Length > 0)
        {
            float intervalo = correndo ? intervaloCorrida : intervaloCaminhada;
            desenho.sprite = quadros[Mathf.FloorToInt(tempo / Mathf.Max(0.02f, intervalo)) % quadros.Length];
        }
        float direcao = emFila ? seguidor.Direcao : Mathf.Sign(deslocamento);
        desenho.flipX = direcao * transform.lossyScale.x < 0f;
    }
}
