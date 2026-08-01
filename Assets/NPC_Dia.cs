using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPC_Dia : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Text dialogueText;
    public string[] dialogue;
    private int index;

    public GameObject contButton;
    public float wordSpeed;
    public bool playerIsClose;

    public GameObject interactPrompt;
    public string sceneToLoad;

    public GameObject playerObject;

    public string[] scriptsToDisable = { "PlayerMovement", "PlayerAttack" };

    void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerIsClose)
        {
            if (dialoguePanel.activeInHierarchy)
            {
                zeroText();
            }
            else
            {
                dialoguePanel.SetActive(true);
                dialogueText.text = "";
                StartCoroutine(Typing());

                if (interactPrompt != null) interactPrompt.SetActive(false);

                TogglePlayerScripts(false);
            }
        }

        if (dialogueText.text == dialogue[index])
        {
            contButton.SetActive(true);
        }
    }

    public void zeroText()
    {
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);

        if (playerIsClose && interactPrompt != null) interactPrompt.SetActive(true);

        TogglePlayerScripts(true);
    }

    IEnumerator Typing()
    {
        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void NextLine()
    {
        contButton.SetActive(false);

        if (index < dialogue.Length - 1)
        {
            index++;
            dialogueText.text = "";
            StartCoroutine(Typing());
        }
        else
        {
            zeroText();
            if (!string.IsNullOrEmpty(sceneToLoad)) SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            zeroText();
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    private void TogglePlayerScripts(bool state)
    {
        if (playerObject != null)
        {
            foreach (string scriptName in scriptsToDisable)
            {
               
                MonoBehaviour script = (MonoBehaviour)playerObject.GetComponent(scriptName);
                if (script != null)
                {
                    script.enabled = state;
                }
            }
        }
    }
}