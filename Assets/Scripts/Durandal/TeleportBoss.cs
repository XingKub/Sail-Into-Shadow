using UnityEngine;
using System.Collections;

public class TeleportBoss : MonoBehaviour
{
    [Header("Timings (Skill 1: Portal Slash)")]
    public float portalInWait = 0.5f;
    public float portalOutWait = 0.5f;
    public float cooldown = 5.0f;

    [Header("Combat (Skill 1: Portal Slash)")]
    [SerializeField] private int teleportDamage = 25;
    public float slashRadius = 3.0f;
    public LayerMask playerLayer;

    [Header("Setup (Skill 1: Portal Slash)")]
    public float activationDistance = 8f;
    public float verticalThreshold = 2.5f;
    public float offset = 1.8f;

    [Header("Skill 2: Aerial Slam Settings")]
    public float slamSkillCooldown = 12.0f;
    public float vanishWait = 0.5f;
    public float slamHeight = 6.0f;
    public float onAirWait = 0.3f;
    public float slamSpeed = 25.0f;
    public int slamDamage = 35;
    public float slamRadius = 4.5f;
    public float landingAnimDuration = 0.8f;
    public GameObject landingEffectPrefab;

    [Header("Skill 3: Catch & Slam Settings")]
    public float catchSlamCooldown = 16.0f;
    public GameObject groundedSwordPrefab;
    public float catchRange = 1.2f;
    public float catchChaseSpeed = 6.5f;
    public float catchTimeout = 3.5f;
    public int catchSlamDamage = 40;
    public Vector2 throwForce = new Vector2(16f, 10f);

    [Header("Audio Settings (Skill 1 & 2)")]
    [SerializeField] private AudioClip portalInSFX;
    [SerializeField] private AudioClip portalOutSlashSFX;
    [SerializeField] private AudioClip vanishSFX;
    [SerializeField] private AudioClip landingImpactSFX;

    [Header("Audio Settings (Skill 3: Catch & Slam)")]
    [SerializeField] private AudioClip throwSwordSFX;
    [SerializeField] private AudioClip chaseRunSFX;
    [SerializeField] private AudioClip catchSlamSFX;
    [SerializeField] private AudioClip recallSwordSFX;

    private float slamCooldownTimer = 0f;
    private float portalCooldownTimer = 0f;
    private float catchSlamCooldownTimer = 0f;

    private DurandalAI ai;
    private Animator anim;
    private Rigidbody2D rb;
    private DurandalHealth health;
    private AudioSource audioSource;
    private bool isExecuting = false;

    public bool IsExecuting => isExecuting;

    void Start()
    {
        ai = GetComponent<DurandalAI>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<DurandalHealth>();
        audioSource = GetComponent<AudioSource>();

        slamCooldownTimer = slamSkillCooldown;
        portalCooldownTimer = cooldown;
        catchSlamCooldownTimer = catchSlamCooldown;
    }

    void Update()
    {
        if (ai.player == null || (health != null && health.HasTakenDamage && health.GetComponent<DurandalHealth>().enabled == false)) return;

        if (slamCooldownTimer > 0f) slamCooldownTimer -= Time.deltaTime;
        if (portalCooldownTimer > 0f) portalCooldownTimer -= Time.deltaTime;
        if (catchSlamCooldownTimer > 0f) catchSlamCooldownTimer -= Time.deltaTime;

        if (isExecuting) return;

        float totalDistance = Vector2.Distance(transform.position, ai.player.position);
        float verticalDistance = Mathf.Abs(transform.position.y - ai.player.position.y);
        bool playerIsFarOrHigh = (totalDistance >= activationDistance || verticalDistance > verticalThreshold);

        bool isGrounded = rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        if (isGrounded && catchSlamCooldownTimer <= 0f)
        {
            catchSlamCooldownTimer = catchSlamCooldown;
            StartCoroutine(CatchAndSlamSequence());
            return;
        }

        if (slamCooldownTimer <= 0f)
        {
            slamCooldownTimer = slamSkillCooldown;
            StartCoroutine(SlamSequence());
            return;
        }

        if (playerIsFarOrHigh && portalCooldownTimer <= 0f)
        {
            portalCooldownTimer = cooldown;
            StartCoroutine(PortalSequence());
            return;
        }
    }

   //skill 3
    IEnumerator CatchAndSlamSequence()
    {
        isExecuting = true;
        ai.isTeleporting = true;
        ai.Stop();
        ai.ResetAttackState();

        if (ai.player != null)
        {
            float directionX = ai.player.position.x - transform.position.x;
            if (Mathf.Abs(directionX) > 0.05f)
            {
                transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            }
        }

        //boss throwsword animation
        anim.Play("ThrowingSwordDugInTheGround");
        if (audioSource != null && throwSwordSFX != null)
        {
            audioSource.PlayOneShot(throwSwordSFX);
        }
        yield return new WaitForSeconds(0.5f);

        //boss spawn sword
        GameObject spawnedSword = null;
        if (groundedSwordPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(transform.localScale.x * 0.7f, -0.2f, 0f);
            spawnedSword = Instantiate(groundedSwordPrefab, spawnPos, Quaternion.identity);

            //sword face direction of the boss
            Vector3 swordScale = spawnedSword.transform.localScale;
            swordScale.x = Mathf.Abs(swordScale.x) * Mathf.Sign(transform.localScale.x);
            spawnedSword.transform.localScale = swordScale;
        }
        yield return new WaitForSeconds(0.2f);

        //boss chase player
        anim.Play("Run");
        if (audioSource != null && chaseRunSFX != null)
        {
            audioSource.clip = chaseRunSFX;
            audioSource.loop = true;
            audioSource.Play();
        }

        float chaseTimeElapsed = 0f;
        bool hasCaughtPlayer = false;

        Transform playerTransform = ai.player;
        PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        Animator playerAnim = playerTransform.GetComponent<Animator>();
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();

        while (chaseTimeElapsed < catchTimeout && playerTransform != null)
        {
            chaseTimeElapsed += Time.deltaTime;

            float directionX = playerTransform.position.x - transform.position.x;
            if (Mathf.Abs(directionX) > 0.05f)
            {
                transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            }

            transform.position = Vector2.MoveTowards(
                transform.position,
                new Vector2(playerTransform.position.x, transform.position.y),
                catchChaseSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, playerTransform.position) <= catchRange)
            {
                if (playerHealth != null && playerHealth.IsInvincible)
                {
                    hasCaughtPlayer = false;
                    break;
                }

                hasCaughtPlayer = true;
                break;
            }
            yield return null;
        }

        //boss stop chase
        if (audioSource != null && audioSource.clip == chaseRunSFX)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        //boss caught player
        if (hasCaughtPlayer && playerTransform != null)
        {
            PlayerAttack playerAttack = playerTransform.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.EndSkillX();
                playerAttack.EndSkillC();
            }

            if (playerMovement != null)
            {
                playerMovement.LockPlayer();
                playerMovement.enabled = false;
            }

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.bodyType = RigidbodyType2D.Kinematic;
            }

            Collider2D[] playerColliders = playerTransform.GetComponents<Collider2D>();
            foreach (Collider2D col in playerColliders)
            {
                if (col != null) col.enabled = false;
            }

            anim.Play("SlamAttack");
            if (audioSource != null && catchSlamSFX != null)
            {
                audioSource.PlayOneShot(catchSlamSFX);
            }

            if (playerAnim != null) playerAnim.Play("GettingSlammed");

            float grabBuildupTimer = 0f;
            while (grabBuildupTimer < 2.5f)
            {
                grabBuildupTimer += Time.deltaTime;
                if (playerTransform != null)
                {
                    float facingDir = transform.localScale.x;
                    playerTransform.position = transform.position + new Vector3(facingDir * 0.75f, 0f, 0f);
                }

                if (playerAnim != null && grabBuildupTimer >= 2.0f)
                {
                    playerAnim.speed = 0f;
                }
                yield return null;
            }

            //throw player and unlock player
            if (playerRb != null) playerRb.bodyType = RigidbodyType2D.Dynamic;

            if (playerMovement != null)
            {
                playerMovement.enabled = true;
                playerMovement.UnlockPlayer();
            }

            if (playerAnim != null)
            {
                playerAnim.speed = 1f;
                playerAnim.Play("Movement");
            }

            foreach (Collider2D col in playerColliders)
            {
                if (col != null) col.enabled = true;
            }

            if (playerHealth != null)
            {
                playerHealth.SetInvincible(false);
                playerHealth.TakeDamage(catchSlamDamage, transform.position);
            }

            if (playerRb != null)
            {
                float throwDirX = transform.localScale.x;
                playerRb.linearVelocity = new Vector2(throwDirX * throwForce.x, throwForce.y);
            }

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
        }

        //boss call sword back
        anim.Play("RecallSword");
        if (audioSource != null && recallSwordSFX != null)
        {
            audioSource.PlayOneShot(recallSwordSFX);
        }

        //vanish sword
        if (spawnedSword != null)
        {
            GroundedSword swordScript = spawnedSword.GetComponent<GroundedSword>();
            if (swordScript != null)
            {
                swordScript.TriggerVanish();
            }
            else
            {
                Destroy(spawnedSword);
            }
        }
        yield return new WaitForSeconds(0.6f);

        if (ai != null)
        {
            ai.isTeleporting = false;
            anim.Play("Movement");
        }
        isExecuting = false;
    }

    IEnumerator PortalSequence()
    {
        isExecuting = true;
        ai.isTeleporting = true;
        ai.ResetAttackState();
        anim.Play("PortalIn");
        if (audioSource != null && portalInSFX != null) { audioSource.PlayOneShot(portalInSFX); }
        yield return new WaitForSeconds(portalInWait);
        if (rb != null && rb.bodyType == RigidbodyType2D.Static) yield break;
        if (ai.player != null)
        {
            float side = ai.player.localScale.x > 0 ? -1 : 1;
            transform.position = new Vector2(ai.player.position.x + (side * offset), ai.player.position.y);
            transform.localScale = new Vector3(ai.player.position.x > transform.position.x ? 1 : -1, 1, 1);
        }
        anim.Play("PortalOut");
        if (audioSource != null && portalOutSlashSFX != null) { audioSource.PlayOneShot(portalOutSlashSFX); }
        yield return new WaitForSeconds(portalOutWait);
        if (ai != null) { ai.isTeleporting = false; anim.Play("Movement"); }
        isExecuting = false;
    }

    IEnumerator SlamSequence()
    {
        isExecuting = true;
        ai.isTeleporting = true;
        ai.Stop();
        ai.ResetAttackState();
        RigidbodyType2D originalBodyType = RigidbodyType2D.Dynamic;
        if (rb != null) { originalBodyType = rb.bodyType; rb.bodyType = RigidbodyType2D.Kinematic; rb.linearVelocity = Vector2.zero; }
        anim.Play("Vanish");
        if (audioSource != null && vanishSFX != null) { audioSource.PlayOneShot(vanishSFX); }
        yield return new WaitForSeconds(vanishWait);
        if (rb != null && rb.bodyType == RigidbodyType2D.Static) yield break;
        if (ai.player != null)
        {
            Vector2 targetGroundPosition = ai.player.position;
            transform.position = new Vector2(targetGroundPosition.x, targetGroundPosition.y + slamHeight);
            transform.localScale = new Vector3(ai.player.position.x > transform.position.x ? 1 : -1, 1, 1);
            anim.Play("OnAir");
            yield return new WaitForSeconds(onAirWait);
            while (transform.position.y > targetGroundPosition.y)
            {
                if (rb != null && rb.bodyType == RigidbodyType2D.Static) yield break;
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, targetGroundPosition.y), slamSpeed * Time.deltaTime);
                yield return null;
            }
            if (rb != null && rb.bodyType == RigidbodyType2D.Static) yield break;
            transform.position = new Vector2(transform.position.x, targetGroundPosition.y);
            anim.Play("BossLanding");
            if (audioSource != null && landingImpactSFX != null) { audioSource.PlayOneShot(landingImpactSFX); }
            if (landingEffectPrefab != null)
            {
                GameObject spawnedEffect = Instantiate(landingEffectPrefab, transform.position, Quaternion.identity);
                Destroy(spawnedEffect, landingAnimDuration);
            }
            ExecuteSlamDamage();
            yield return new WaitForSeconds(landingAnimDuration);
        }
        if (rb != null && rb.bodyType != RigidbodyType2D.Static) { rb.bodyType = originalBodyType; }
        if (ai != null) { ai.isTeleporting = false; anim.Play("Movement"); }
        isExecuting = false;
    }

    public void BossSlash()
    {
        if (rb != null && rb.bodyType == RigidbodyType2D.Static) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slashRadius, playerLayer);
        bool hitSomething = false;
        foreach (Collider2D hit in hits)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>() ?? hit.GetComponentInParent<PlayerHealth>();
            if (ph != null) { ph.TakeDamage(teleportDamage, transform.position); hitSomething = true; }
        }
        if (!hitSomething && playerObj != null)
        {
            if (Vector2.Distance(transform.position, playerObj.transform.position) <= slashRadius)
            {
                playerObj.GetComponent<PlayerHealth>()?.TakeDamage(teleportDamage, transform.position);
            }
        }
    }

    private void ExecuteSlamDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>() ?? hit.GetComponentInParent<PlayerHealth>();
            if (ph != null) { ph.TakeDamage(slamDamage, transform.position); }
        }
    }
}