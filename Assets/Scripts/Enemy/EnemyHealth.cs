using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Header("Drop Settings")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField, Range(0f, 100f)] private float dropChance = 10f; //heal drop chance
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;
    private bool isDead = false;

    public bool HasTakenDamage { get; set; }

    [Header("Blink Settings")]
    [SerializeField] private int blinkAmount = 3;
    [SerializeField] private float blinkInterval = 0.05f;

    [Header("Knockback Settings")]
    [SerializeField] private bool giveKnockback = true; //knockback

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private EnemyAI2D aiScript;

    public bool IsBeingKnockedBack { get; private set; }

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        aiScript = GetComponent<EnemyAI2D>();
    }

    public void Damage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        HasTakenDamage = true;

        

        StopCoroutine(nameof(BlinkRoutine));
        StartCoroutine(nameof(BlinkRoutine));

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (anim != null)
            {
                anim.ResetTrigger("Attack");
                anim.SetTrigger("Hurt");
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < blinkAmount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public void TakeKnockback(float force, Vector2 sourcePosition)
    {
        if (!giveKnockback || IsBeingKnockedBack || rb == null || isDead) return;

        Vector2 direction = (Vector2)transform.position - sourcePosition;
        direction = new Vector2(direction.x, 0.2f).normalized;

        StartCoroutine(KnockbackRoutine(direction, force));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        IsBeingKnockedBack = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f);

        IsBeingKnockedBack = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        StopCoroutine(nameof(BlinkRoutine));
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        if (anim != null) anim.SetBool("isDead", true);

        if (aiScript != null) aiScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        //drop heal
        if (healthPickupPrefab != null)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= dropChance)
            {
                Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject, 1.5f);
    }
}