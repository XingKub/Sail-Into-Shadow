using UnityEngine;

public class AirSlashEffect : MonoBehaviour
{
    public float damage = 2f;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 0.5f; //duration
    [SerializeField] private LayerMask enemyLayer;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip impactSound;
    [Range(0.1f, 3f)][SerializeField] private float minPitch = 0.85f;
    [Range(0.1f, 3f)][SerializeField] private float maxPitch = 1.15f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        //slash on the way player facing
        float direction = transform.localScale.x > 0 ? 1 : -1;
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable dmg = collision.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.Damage(damage);
            dmg.HasTakenDamage = false;

            if (impactSound != null)
            {
                PlayImpactSFX(impactSound, transform.position);
            }

            if (collision.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeKnockback(10f, transform.position);
            }
        }
    }

    //sfx
    private void PlayImpactSFX(AudioClip clip, Vector3 position)
    {
        GameObject tempAudioObj = new GameObject("TempAudio");
        tempAudioObj.transform.position = position;

        AudioSource source = tempAudioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = Random.Range(minPitch, maxPitch);

        // Match standard 2D settings
        source.spatialBlend = 0f;
        source.Play();

        Destroy(tempAudioObj, clip.length);
    }
}