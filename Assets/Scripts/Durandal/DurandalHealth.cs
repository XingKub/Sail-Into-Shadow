using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DurandalHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("UI Settings")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject healthBarUI;

    public bool HasTakenDamage { get; set; }

    [Header("Blink Settings")]
    [SerializeField] private int blinkAmount = 3;
    [SerializeField] private float blinkInterval = 0.05f;

    [Header("Knockback Settings")]
    [SerializeField] private bool giveKnockback = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip deathSFX;

    // --- ส่วนที่เพิ่มใหม่: ตั้งค่าสำหรับการเปลี่ยนฉากหลังบอสตาย ---
    [Header("Scene Transition Settings")]
    [SerializeField] private string nextSceneName = "VictoryScene"; // ชื่อฉากที่ต้องการให้โหลดไป
    [SerializeField] private float delayBeforeLoad = 3.0f;          // ดีเลย์รอให้แอนิเมชัน/เสียงตายของบอสเล่นจบ (วินาที)

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private DurandalAI aiScript;
    private TeleportBoss teleportScript;
    private AudioSource audioSource;

    public bool IsBeingKnockedBack { get; private set; }

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        aiScript = GetComponent<DurandalAI>();
        teleportScript = GetComponent<TeleportBoss>();
        audioSource = GetComponent<AudioSource>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void Damage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        HasTakenDamage = true;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        StopCoroutine(nameof(BlinkRoutine));
        StartCoroutine(nameof(BlinkRoutine));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator BlinkRoutine()
    {
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

        // Play Death SFX
        if (audioSource != null && deathSFX != null)
        {
            audioSource.PlayOneShot(deathSFX);
        }

        if (healthBarUI != null) healthBarUI.SetActive(false);

        StopCoroutine(nameof(BlinkRoutine));
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        if (anim != null) anim.SetBool("isDead", true);
        if (aiScript != null) aiScript.enabled = false;

        if (teleportScript != null)
        {
            teleportScript.StopAllCoroutines();
            teleportScript.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // --- เพิ่มคำสั่งให้เริ่มนับเวลาถอยหลังเพื่อเปลี่ยนฉาก ---
        StartCoroutine(LoadNextSceneRoutine());
    }

    // --- ส่วนที่เพิ่มใหม่: ฟังก์ชัน Coroutine สำหรับรอเวลาแล้วเปลี่ยนฉาก ---
    private IEnumerator LoadNextSceneRoutine()
    {
        // รอเวลาตามที่กำหนดไว้ใน Inspector (ให้บอสเล่นท่าตายเสร็จก่อน)
        yield return new WaitForSeconds(delayBeforeLoad);

        // เปลี่ยนไปยัง Scene ใหม่
        SceneManager.LoadScene(nextSceneName);
    }
}