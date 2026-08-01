using UnityEngine;

public class DurandalAI : MonoBehaviour
{
    [Header("Damage & Combat")]
    [SerializeField] private int damage = 10;
    public float heightTolerance = 1f;
    public Transform player;
    public GameObject damageZonePrefab;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float followRange = 10f;
    public bool isTeleporting = false;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;
    private bool isAttacking = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip normalAttackSFX;

    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;
    private float stuckAttackTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isAttacking)
        {
            stuckAttackTimer += Time.deltaTime;
            if (stuckAttackTimer > 2.0f)
            {
                ResetAttackState();
            }
        }
        else
        {
            stuckAttackTimer = 0f;
        }

        if (isTeleporting || isAttacking)
        {
            Stop();
            return;
        }

        if (player == null)
        {
            Stop();
            return;
        }

        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);

        if (Vector2.Distance(transform.position, player.position) <= followRange)
        {
            if (distanceX > attackRange)
            {
                Move(player.position.x - transform.position.x);
            }
            else if (distanceY <= heightTolerance)
            {
                Stop();
                Attack();
            }
            else
            {
                Stop();
            }
        }
        else
        {
            Stop();
        }
    }

    void Move(float dirX)
    {
        float move = Mathf.Sign(dirX);
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
        anim.SetFloat("xVelocity", Mathf.Abs(move));
        transform.localScale = new Vector3(move > 0 ? 1 : -1, 1, 1);
    }

    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("xVelocity", 0f);
    }

    void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            anim.SetTrigger("Attack");

            if (audioSource != null && normalAttackSFX != null)
            {
                audioSource.PlayOneShot(normalAttackSFX);
            }

            lastAttackTime = Time.time;
        }
    }

    public void ResetAttackState()
    {
        isAttacking = false;
    }

    public void TriggerMeleeDamage()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRange + 0.5f)
            {
                player.GetComponent<PlayerHealth>()?.TakeDamage(damage, transform.position);
            }
        }
    }

    public void SpawnDamageZone()
    {
        if (damageZonePrefab != null)
        {
            Instantiate(damageZonePrefab, transform.position, Quaternion.identity);
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }
}