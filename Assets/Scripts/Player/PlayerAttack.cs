using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackTransform;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float timeBtwAttacks = 0.15f;
    [SerializeField] private float knockbackForce = 10f;

    [Header("Skill Settings")]
    [SerializeField] private GameObject airSlashPrefab;
    [SerializeField] private float skillDamage = 2f;
    [SerializeField] private float skillCooldown = 1f;
    [SerializeField] private float airStallForce = 2f;
    private float skillTimeCounter;

    [Header("Skill X Advanced Settings")]
    [SerializeField] private float skillXDamage = 0.8f;
    [SerializeField] private float skillXCooldown = 3f;
    [SerializeField] private Vector2 skillXHitboxSize = new Vector2(4f, 3f);
    [SerializeField] private Vector2 skillXHitboxOffset = new Vector2(1f, 0f);
    [SerializeField] private AudioClip skillXSound;
    private float skillXTimeCounter;
    private bool isUsingSkillX = false;

    [Header("Skill C Advanced Settings")]
    [SerializeField] private float skillCDamage = 0.8f;
    [SerializeField] private float skillCCooldown = 3f;
    [SerializeField] private Vector2 skillCHitboxSize = new Vector2(4f, 3f);
    [SerializeField] private Vector2 skillCHitboxOffset = new Vector2(1f, 0f);
    [SerializeField] private AudioClip skillCSound;
    private float skillCTimeCounter;
    private bool isUsingSkillC = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip attackLandedSound;
    [SerializeField] private AudioClip airSlashSound;
    [Range(0.1f, 3f)][SerializeField] private float minPitch = 0.85f;
    [Range(0.1f, 3f)][SerializeField] private float maxPitch = 1.15f;

    [Header("Components")]
    [SerializeField] private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement movement;

    public bool ShouldBeDamaging { get; private set; } = false;
    private List<IDamageable> iDamageables = new List<IDamageable>();
    private float attackTimeCounter;
    private PlayerControls controls;

    private void Awake()
    {
        anim = anim ?? GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        audioSource = audioSource ?? GetComponent<AudioSource>();
        SetupControls();
    }

    private void SetupControls()
    {
        controls = new PlayerControls();
        controls.Player.Attack.performed += ctx => TryAttack();
        controls.Player.AirSlash.performed += ctx => TryAirSlashSkill();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                if (Time.time >= skillXTimeCounter && !isUsingSkillX && !isUsingSkillC && (movement == null || movement.canMove))
                {
                    ExecuteSkillXStart();
                }
            }

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                if (Time.time >= skillCTimeCounter && !isUsingSkillX && !isUsingSkillC && (movement == null || movement.canMove))
                {
                    ExecuteSkillCStart();
                }
            }
        }
    }

    private void TryAttack()
    {
        if (isUsingSkillX || isUsingSkillC) return;

        if (Time.time >= attackTimeCounter)
        {
            anim.SetTrigger("Attack");
            attackTimeCounter = Time.time + timeBtwAttacks;

            if (audioSource != null && attackSound != null)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(attackSound);
            }
        }
    }

    private void TryAirSlashSkill()
    {
        if (isUsingSkillX || isUsingSkillC) return;

        if (Time.time >= skillTimeCounter)
        {
            anim.SetTrigger("AirSlash");

            if (audioSource != null && airSlashSound != null)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(airSlashSound);
            }

            if (airSlashPrefab != null)
            {
                GameObject slash = Instantiate(airSlashPrefab, attackTransform.position, Quaternion.identity);
                AirSlashEffect effectScript = slash.GetComponent<AirSlashEffect>();
                if (effectScript != null)
                {
                    effectScript.damage = skillDamage;
                }

                Vector3 s = slash.transform.localScale;
                s.x = transform.localScale.x;
                slash.transform.localScale = s;
            }

            if (movement != null && !movement.IsGrounded() && rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, airStallForce);
            }

            skillTimeCounter = Time.time + skillCooldown;
        }
    }

    //skill x
    private void ExecuteSkillXStart()
    {
        isUsingSkillX = true;
        skillXTimeCounter = Time.time + skillXCooldown;

        if (movement != null) movement.LockPlayer();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null) playerHealth.SetInvincible(true);

        anim.SetTrigger("SkillX");
        if (audioSource != null && skillXSound != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(skillXSound);
        }
    }

    public void DealSkillXHitDamage()
    {
        float facingDir = Mathf.Sign(transform.localScale.x);
        Vector2 boxCenter = (Vector2)attackTransform.position + new Vector2(skillXHitboxOffset.x * facingDir, skillXHitboxOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, skillXHitboxSize, 0f, attackableLayer);
        List<IDamageable> affectedInThisHit = new List<IDamageable>();

        foreach (Collider2D hitCollider in hits)
        {
            IDamageable dmg = hitCollider.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.Damage(skillXDamage);
                affectedInThisHit.Add(dmg);

                if (hitCollider.TryGetComponent(out EnemyHealth enemyHealth))
                {
                    enemyHealth.TakeKnockback(knockbackForce * 0.4f, transform.position);
                }

                if (audioSource != null && attackLandedSound != null)
                {
                    audioSource.pitch = Random.Range(minPitch, maxPitch);
                    audioSource.PlayOneShot(attackLandedSound);
                }
            }
        }

        foreach (IDamageable d in affectedInThisHit)
        {
            if (d != null) d.HasTakenDamage = false;
        }
    }

    public void EndSkillX()
    {
        if (movement != null) movement.UnlockPlayer();

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null) playerHealth.SetInvincible(false);

        isUsingSkillX = false;
    }

    //skill c
    private void ExecuteSkillCStart()
    {
        isUsingSkillC = true;
        skillCTimeCounter = Time.time + skillCCooldown;

        if (movement != null) movement.LockPlayer();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null) playerHealth.SetInvincible(true);

        anim.SetTrigger("SkillC");
        if (audioSource != null && skillCSound != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(skillCSound);
        }
    }

    public void DealSkillCHitDamage()
    {
        float facingDir = Mathf.Sign(transform.localScale.x);
        Vector2 boxCenter = (Vector2)attackTransform.position + new Vector2(skillCHitboxOffset.x * facingDir, skillCHitboxOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, skillCHitboxSize, 0f, attackableLayer);
        List<IDamageable> affectedInThisHit = new List<IDamageable>();

        foreach (Collider2D hitCollider in hits)
        {
            IDamageable dmg = hitCollider.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.Damage(skillCDamage);
                affectedInThisHit.Add(dmg);

                if (hitCollider.TryGetComponent(out EnemyHealth enemyHealth))
                {
                    enemyHealth.TakeKnockback(knockbackForce * 0.4f, transform.position);
                }

                if (audioSource != null && attackLandedSound != null)
                {
                    audioSource.pitch = Random.Range(minPitch, maxPitch);
                    audioSource.PlayOneShot(attackLandedSound);
                }
            }
        }

        foreach (IDamageable d in affectedInThisHit)
        {
            if (d != null) d.HasTakenDamage = false;
        }
    }

    public void EndSkillC()
    {
        if (movement != null) movement.UnlockPlayer();

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null) playerHealth.SetInvincible(false);

        isUsingSkillC = false;
    }

    //skill z , normal atk
    private IEnumerator DamageWhileSlashIsActive()
    {
        ShouldBeDamaging = true;

        while (ShouldBeDamaging)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackTransform.position, attackRange, attackableLayer);

            for (int i = 0; i < hits.Length; i++)
            {
                IDamageable dmg = hits[i].GetComponent<IDamageable>();
                if (dmg != null && !dmg.HasTakenDamage)
                {
                    float finalDamage = anim.GetCurrentAnimatorStateInfo(0).IsName("AirSlash") ? skillDamage : damageAmount;

                    dmg.Damage(finalDamage);
                    dmg.HasTakenDamage = true;
                    iDamageables.Add(dmg);

                    if (hits[i].TryGetComponent(out EnemyHealth enemyHealth))
                    {
                        enemyHealth.TakeKnockback(knockbackForce, transform.position);
                    }

                    // --- เพิ่มโค้ดส่วนนี้กลับเข้าไป เพื่อให้มีเสียง Hit Landed ตอนตีปกติโดนศัตรู ---
                    if (audioSource != null && attackLandedSound != null)
                    {
                        audioSource.pitch = Random.Range(minPitch, maxPitch);
                        audioSource.PlayOneShot(attackLandedSound);
                    }
                }
            }
            yield return null;
        }
        ResetDamageables();
    }

    private void ResetDamageables()
    {
        foreach (IDamageable d in iDamageables) if (d != null) d.HasTakenDamage = false;
        iDamageables.Clear();
    }

    public void StartDamage() { StartCoroutine(DamageWhileSlashIsActive()); }
    public void StopDamage() { ShouldBeDamaging = false; }

    private void OnEnable() { controls?.Enable(); }
    private void OnDisable() { controls?.Disable(); }

    private void OnDrawGizmosSelected()
    {
        if (attackTransform != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(attackTransform.position, attackRange);

            float facingDir = Mathf.Sign(transform.localScale.x);

            Gizmos.color = Color.red;
            Vector2 boxXCenter = (Vector2)attackTransform.position + new Vector2(skillXHitboxOffset.x * facingDir, skillXHitboxOffset.y);
            Gizmos.DrawWireCube(boxXCenter, skillXHitboxSize);

            Gizmos.color = Color.yellow;
            Vector2 boxCCenter = (Vector2)attackTransform.position + new Vector2(skillCHitboxOffset.x * facingDir, skillCHitboxOffset.y);
            Gizmos.DrawWireCube(boxCCenter, skillCHitboxSize);
        }
    }
}