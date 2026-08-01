using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 10;
    [SerializeField] private PlayerMovement player;

    private Queue<string> paragraphs = new Queue<string>();

    private bool isTyping;
    private bool conversationEnded;
    private string currentText;
    private Coroutine typingCoroutine;

    public void DisplayNextParagraph(DialogueText dialogue)
    {
        if (paragraphs.Count == 0)
        {
            if (!conversationEnded)
            {
                StartConversation(dialogue);
            }
            else if (!isTyping)
            {
                EndConversation();
                return;
            }
        }

        if (!isTyping)
        {
            currentText = paragraphs.Dequeue();
            typingCoroutine = StartCoroutine(TypeText(currentText));
        }
        else
        {
            FinishInstant();
        }

        if (paragraphs.Count == 0)
            conversationEnded = true;
    }

    private void StartConversation(DialogueText dialogue)
    {
        gameObject.SetActive(true);

        if (player != null)
            player.LockPlayer();

        NPCNameText.text = dialogue.speakerName;

        foreach (string p in dialogue.paragraphs)
            paragraphs.Enqueue(p);
    }

    private void EndConversation()
    {
        paragraphs.Clear();
        conversationEnded = false;

        if (player != null)
            player.UnlockPlayer();

        gameObject.SetActive(false);
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        NPCDialogueText.text = "";

        foreach (char c in text)
        {
            NPCDialogueText.text += c;
            yield return new WaitForSeconds(0.1f / typeSpeed);
        }

        isTyping = false;
    }

    private void FinishInstant()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        NPCDialogueText.text = currentText;
        isTyping = false;
    }
}