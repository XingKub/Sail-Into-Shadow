using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string spawnName;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && SceneData.spawnPointName == spawnName)
        {
            player.transform.position = transform.position;
        }
    }
}