using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Text dialogueText;
    public string[] dialogue;
    private int index = 0;

    public Button contButton; // 🟢 เปลี่ยนจาก GameObject เป็น Button เพื่อสั่งงานง่ายขึ้น
    public float wordSpeed;

    [Header("Player Settings")]
    public GameObject playerObject;
    public string[] scriptsToDisable = { "PlayerMovement", "PlayerAttack" };

    [Header("Sequence Settings")]
    public bool activateAtStart = false;
    public float startDelay = 2.0f;
    public GameObject nextTutorial;

    private bool hasTriggered = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (activateAtStart)
        {
            hasTriggered = true;
            TogglePlayerScripts(false);
            Time.timeScale = 0f;
            StartCoroutine(DelayedStart());
        }
    }

    void OnEnable()
    {
        index = 0;
        // 🟢 เมื่อ Tutorial นี้ถูกเปิด (เช่น Jump) ให้มันไปเคลียร์คำสั่งเก่าในปุ่ม และใส่ NextLine ของตัวเองเข้าไปแทน
        if (contButton != null)
        {
            contButton.onClick.RemoveAllListeners();
            contButton.onClick.AddListener(NextLine);
            contButton.gameObject.SetActive(false);
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSecondsRealtime(startDelay);
        StartTutorial();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = "";
        TogglePlayerScripts(false);
        Time.timeScale = 0f;

        if (gameObject.activeInHierarchy)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(Typing());
        }
    }

    IEnumerator Typing()
    {
        dialogueText.text = "";
        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(wordSpeed);
        }
        if (contButton != null) contButton.gameObject.SetActive(true);
        typingCoroutine = null;
    }

    public void NextLine()
    {
        if (typingCoroutine != null) return;

        if (contButton != null) contButton.gameObject.SetActive(false);

        if (index < dialogue.Length - 1)
        {
            index++;
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(Typing());
        }
        else
        {
            EndTutorial();
        }
    }

    void EndTutorial()
    {
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        TogglePlayerScripts(true);
        Time.timeScale = 1f;

        if (nextTutorial != null)
        {
            nextTutorial.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private void TogglePlayerScripts(bool state)
    {
        if (playerObject != null)
        {
            foreach (string scriptName in scriptsToDisable)
            {
                MonoBehaviour script = (MonoBehaviour)playerObject.GetComponent(scriptName);
                if (script != null) script.enabled = state;
            }
        }
    }
}