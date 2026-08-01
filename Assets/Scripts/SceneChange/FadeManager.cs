using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.5f;
    public float delayBeforeFadeIn = 0.5f;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void StartTransition(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeIn()
    {
        fadeImage.color = new Color(0, 0, 0, 1);

        yield return new WaitForSeconds(delayBeforeFadeIn);

        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1 - (t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}