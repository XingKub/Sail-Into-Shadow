using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public int damagePerTick = 5;
    public float tickRate = 1.0f; //damage every sec
    private float nextTickTime;

    void Start()
    {
        Destroy(gameObject, 10f); //duration 10 sec
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time >= nextTickTime)
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damagePerTick, transform.position);
                nextTickTime = Time.time + tickRate;
            }
        }
    }
}