using UnityEngine;

public class SceneExit : MonoBehaviour
{
    public string nextScene;
    public string spawnPointName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Stop player movement
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
                player.canMove = false;

            // Save spawn point for next scene
            SceneData.spawnPointName = spawnPointName;

            // Fade
            FadeManager fade = FindFirstObjectByType<FadeManager>();
            if (fade != null)
                fade.StartTransition(nextScene);
        }
    }
}