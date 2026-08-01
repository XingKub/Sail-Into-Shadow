using UnityEngine;

public class BackgroundController_F : MonoBehaviour
{
    private float startPos;
    public GameObject cam;

    [Range(-1, 1)]
    public float parallaxEffect;

    void Start()
    {
        startPos = transform.position.x;
    }

    void LateUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect;

        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
    }
}