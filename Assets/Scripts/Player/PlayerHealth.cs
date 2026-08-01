using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthBar;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 14f;
    [SerializeField] private float knockbackUpForce = 8f;

    [Header("Invincibility & Visuals")]
    [SerializeField] private float invincibleTime = 1.0f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Death UI")]
    [SerializeField] private GameObject deadScreenObject;
    [SerializeField] private StageFailedUI stageFailedScreen; // ช่องสำหรับลากหน้า Stage Failed มาใส่

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip youDeadSFX; // ช่องสำหรับใส่เสียงตอนตาย
    [Range(0.1f, 3f)][SerializeField] private float minPitch = 0.9f;
    [Range(0.1f, 3f)][SerializeField] private float maxPitch = 1.1f;

    private float currentHealth;
    private bool isDead = false;
    private bool isInvincible = false;

    private Animator animator;
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;

    // เปิดให้สคริปต์อื่น (เช่น บอส) สามารถเข้ามาเช็คสถานะอมตะได้แบบปลอดภัย
    public bool IsInvincible => isInvincible;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (healthBar != null) healthBar.value = 1f;

        if (deadScreenObject != null) deadScreenObject.SetActive(false);
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isDead || isInvincible) return; // ถ้าร่างอมตะอยู่ จะไม่โดนดาเมจตรงนี้ทันที

        currentHealth -= damage;

        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;

        if (audioSource != null && hurtSound != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(hurtSound);
        }

        ApplyKnockback(attackerPosition, damage);
        StartCoroutine(InvincibilityRoutine());

        if (currentHealth <= 0) Die();
    }

    private void ApplyKnockback(Vector2 attackerPosition, int damageReceived)
    {
        float xDir = transform.position.x - attackerPosition.x;
        xDir = (Mathf.Abs(xDir) < 0.1f) ? (transform.localScale.x > 0 ? -1f : 1f) : Mathf.Sign(xDir);

        if (movement != null) movement.SetHurt(true);

        if (animator != null)
        {
            animator.ResetTrigger("hurtTrigger");
            animator.ResetTrigger("hurtALotTrigger");

            if (damageReceived >= 30)
                animator.SetTrigger("hurtALotTrigger");
            else
                animator.SetTrigger("hurtTrigger");
        }

        rb.linearVelocity = new Vector2(xDir * knockbackForce, knockbackUpForce);
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibleTime)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2;
            if (timer > 0.25f && movement != null) movement.SetHurt(false);
        }
        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. เปิดหน้า You Dead อันเก่า
        if (deadScreenObject != null)
        {
            deadScreenObject.SetActive(true);
        }

        // 2. เล่นเสียงตอนตายทันที
        if (audioSource != null && youDeadSFX != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(youDeadSFX);
        }

        // 3. รอ 2 วินาทีแบบ Realtime (ไม่โดนแช่แข็งจาก Time.timeScale) แล้วขึ้นหน้า Stage Failed
        StartCoroutine(ShowStageFailedAfterDelay(2.0f));

        if (animator != null)
        {
            animator.SetBool("isDead", true);
            animator.SetTrigger("hurtALotTrigger");
        }

        if (movement != null) movement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private IEnumerator ShowStageFailedAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (stageFailedScreen != null)
        {
            stageFailedScreen.ShowStageFailedScreen();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}