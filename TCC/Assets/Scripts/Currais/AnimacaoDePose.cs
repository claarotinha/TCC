using System;
using UnityEngine;

// As poses e seus quadros são editáveis na Hierarchy. A narrativa continua escolhendo a pose base.
[DefaultExecutionOrder(500)]
public class AnimacaoDePose : MonoBehaviour
{
    [Serializable] public class Parte
    {
        public SpriteRenderer desenho;
        public Sprite[] quadros;
    }
    [Serializable] public class Estado
    {
        public Sprite[] poses;
        public Parte[] partes;
    }
    [SerializeField] private SpriteRenderer origem;
    [SerializeField] private Estado[] estados;
    [SerializeField] private float intervalo = .22f;
    private float tempo;
    private Estado anterior;

    private void LateUpdate()
    {
        if (origem == null || estados == null) return;
        Estado atual = null;
        foreach (Estado estado in estados)
            foreach (Sprite pose in estado.poses)
                if (pose != null && origem.sprite == pose) atual = estado;
        if (anterior != atual) tempo = 0f;
        if (!PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
        origem.forceRenderingOff = atual != null;
        foreach (Estado estado in estados)
            foreach (Parte parte in estado.partes)
            {
                if (parte.desenho == null) continue;
                bool visivel = estado == atual && origem.enabled && parte.quadros.Length > 0;
                parte.desenho.enabled = visivel;
                if (!visivel) continue;
                parte.desenho.sprite = parte.quadros[Mathf.FloorToInt(tempo / Mathf.Max(.05f, intervalo)) % parte.quadros.Length];
                parte.desenho.color = origem.color;
                parte.desenho.sortingLayerID = origem.sortingLayerID;
                parte.desenho.sortingOrder = origem.sortingOrder;
            }
        anterior = atual;
    }
    private void OnDisable()
    {
        if (origem != null) origem.forceRenderingOff = false;
        if (estados == null) return;
        foreach (Estado estado in estados)
            foreach (Parte parte in estado.partes)
                if (parte.desenho != null) parte.desenho.enabled = false;
    }
}
