using UnityEngine;

public class FinishPoint_2 : MonoBehaviour
{
    [SerializeField] bool goNextLevel;
    [SerializeField] string levelName;


    private void OnTriggerEnter2D(Collider2D collision)

    {
        if (collision.CompareTag("Player"))
        {
            if (goNextLevel)
            {
                SceneController_2.instance.NextLevel();
            }
            else
            {
                SceneController_2.instance.LoadScene(levelName);
            }
                
        }
    }
}