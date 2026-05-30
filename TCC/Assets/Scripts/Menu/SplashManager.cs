using UnityEngine;
using System.Collections;

public class SplashManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);

        GameManager.Instance.LoadScene("MenuPrincipal");
    }
}