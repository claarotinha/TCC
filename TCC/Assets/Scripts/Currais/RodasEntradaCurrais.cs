using UnityEngine;

// Roda somente enquanto a carroça se desloca. Cada roda continua editável na Hierarchy.
public sealed class RodasEntradaCurrais : MonoBehaviour
{
    [SerializeField] private Transform[] rodas;
    [SerializeField, Min(.01f)] private float raioLocal = .55f;
    private float xAnterior;
    private void OnEnable() => xAnterior = transform.position.x;
    private void LateUpdate()
    {
        float delta = transform.position.x - xAnterior;
        xAnterior = transform.position.x;
        if (PauseHelper.BlockInput() || Time.timeScale <= 0f || Mathf.Abs(delta) < .00001f) return;
        float raio = Mathf.Max(.01f, raioLocal * Mathf.Abs(transform.lossyScale.x));
        float graus = -delta / raio * Mathf.Rad2Deg;
        if (rodas == null) return;
        foreach (Transform roda in rodas)
            if (roda != null) roda.Rotate(0f, 0f, graus, Space.Self);
    }
}
