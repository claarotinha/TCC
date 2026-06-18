using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public float cutsceneDuration = 4f;   // duração em segundos

    private void Start()
    {
        StartCoroutine(RunCutscene());
    }

    private IEnumerator RunCutscene()
    {
        // Aqui você pode adicionar sua lógica de cutscene (diálogos, animações)
        Debug.Log("Cutscene iniciada...");

        yield return new WaitForSeconds(cutsceneDuration);

        // Descarrega a cena da cutscene
        SceneManager.UnloadSceneAsync(gameObject.scene);

        // Aguarda um frame para garantir a descarga
        yield return null;

        // Chama o retorno para a cena original
        DiaryWithCutscene.ReturnToGame();
    }
}