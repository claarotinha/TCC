using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DiaryReturnHandler : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == DiaryWithCutscene.savedSceneName && DiaryWithCutscene.cutsceneWatched)
        {
            StartCoroutine(RestoreAfterFrame());
        }
    }

    private IEnumerator RestoreAfterFrame()
    {
        yield return null; // espera 1 frame para o jogador ser instanciado
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = DiaryWithCutscene.savedPlayerPosition;
            Debug.Log("Posição restaurada: " + player.transform.position);
        }
        else
        {
            Debug.LogWarning("Jogador com tag 'Player' não encontrado.");
        }
    }
}