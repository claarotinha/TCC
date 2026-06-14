using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class HouseEntrance : MonoBehaviour
{
    public GameObject doorText;

    public Image fadeImage;
    public float fadeDuration = 1f;

    private bool playerNearby = false;
    private bool isTransitioning = false;

    void Update()
    {
        if (playerNearby && !isTransitioning && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (doorText != null)
            {
                doorText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (doorText != null)
            {
                doorText.SetActive(false);
            }
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        isTransitioning = true;

        if (doorText != null)
        {
            doorText.SetActive(false);
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                fadeImage.color = color;
            }

            yield return null;
        }

        SceneManager.LoadScene("Casa_Manha");
    }
}