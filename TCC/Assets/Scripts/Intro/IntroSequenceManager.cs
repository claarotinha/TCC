using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSequenceManager : MonoBehaviour
{
    [Header("Painéis")]
    public CanvasGroup productionPanel;
    public CanvasGroup warningPanel;
    public CanvasGroup fictionPanel;

    [Header("Configurações")]
    public float fadeTime = 1f;
    public float displayTime = 4f;

    [Header("Próxima Cena")]
    public string nextScene = "CutsceneInicial";

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        yield return PlayPanel(productionPanel);

        yield return PlayPanel(warningPanel);

        yield return PlayPanel(fictionPanel);

        SceneManager.LoadScene(nextScene);
    }

    IEnumerator PlayPanel(CanvasGroup panel)
    {
        productionPanel.gameObject.SetActive(false);
        warningPanel.gameObject.SetActive(false);
        fictionPanel.gameObject.SetActive(false);

        panel.gameObject.SetActive(true);

        panel.alpha = 0;

        // Fade In
        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            panel.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            yield return null;
        }

        panel.alpha = 1;

        yield return new WaitForSeconds(displayTime);

        // Fade Out

        t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            panel.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            yield return null;
        }

        panel.alpha = 0;

        panel.gameObject.SetActive(false);
    }
}