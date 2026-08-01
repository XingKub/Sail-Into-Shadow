using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject cutscenePanel;

    [Header("Cutscene UI Elements")]
    public Image cutsceneImageComponent;
    public GameObject nextButtonObject;

    [Header("Cutscene Settings")]
    public Sprite[] cutsceneImages;
    public string gameSceneName = "GameScene";

    [Header("Timing & Zoom")]
    public float totalCutsceneDuration = 20f;
    public float zoomPercentPerSecond = 1f;

    private int currentFrameIndex = 0;
    private float timePerFrame;
    private Coroutine cutsceneCoroutine;

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);

        if (cutsceneImages != null && cutsceneImages.Length > 0)
        {
            timePerFrame = totalCutsceneDuration / cutsceneImages.Length;
        }
    }

    public void StartGame()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(true);
        if (nextButtonObject != null) nextButtonObject.SetActive(false);

        currentFrameIndex = 0;

        if (cutsceneCoroutine != null) StopCoroutine(cutsceneCoroutine);
        cutsceneCoroutine = StartCoroutine(PlayCutsceneSequence());
    }

    IEnumerator PlayCutsceneSequence()
    {
        while (cutsceneImages != null && currentFrameIndex < cutsceneImages.Length)
        {
            if (cutsceneImageComponent != null)
            {
                cutsceneImageComponent.sprite = cutsceneImages[currentFrameIndex];
                cutsceneImageComponent.rectTransform.localScale = Vector3.one;
            }

            float elapsed = 0f;
            Vector3 initialScale = Vector3.one;

            while (elapsed < timePerFrame)
            {
                elapsed += Time.deltaTime;
                float currentZoomFactor = 1f + (elapsed * (zoomPercentPerSecond / 100f));

                if (cutsceneImageComponent != null)
                {
                    cutsceneImageComponent.rectTransform.localScale = initialScale * currentZoomFactor;
                }

                yield return null;
            }

            //move to next frame
            currentFrameIndex++;
        }

        if (nextButtonObject != null)
        {
            nextButtonObject.SetActive(true);
        }
    }
    public void NextCutsceneFrameManual()
    {
        TransitionToGame();
    }

    void TransitionToGame()
    {
        if (cutsceneCoroutine != null) StopCoroutine(cutsceneCoroutine);
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}