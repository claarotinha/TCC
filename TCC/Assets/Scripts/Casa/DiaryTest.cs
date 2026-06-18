using UnityEngine;

public class DiaryTest : MonoBehaviour
{
    void Update()
    {
        // 1. Verifica se o script está rodando
        Debug.Log("Script está rodando!");

        // 2. Verifica se a câmera existe
        if (Camera.main == null)
        {
            Debug.LogError("Camera.main é null!");
            return;
        }

        // 3. Lança o raio
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 0.1f);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Raycast acertou: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log(">>> ACERTOU O DIÁRIO <<<");
            }
        }
        else
        {
            Debug.Log("Raycast não acertou nada.");
        }
    }
}