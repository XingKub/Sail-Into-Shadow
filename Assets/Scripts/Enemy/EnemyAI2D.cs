using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    public float heightTolerance = 1f;
    public Transform player;
    private Rigidbody2D rb;
    private EnemyHealth health;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float followRange = 10f;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;
    private bool isAttacking = false;

    [Header("Animation")]
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        bool isHurt = anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt");

        if ((health != null && health.IsBeingKnockedBack) || isHurt)
        {
 
            isAttacking = false;
            return;
        }

        if (isAttacking && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            isAttacking = false;
        }

        if (isAttacking)
        {
            Stop();
            return;
        }

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);

        if (distance <= followRange)
        {
            float directionX = player.position.x - transform.position.x;

            if (distanceX > attackRange)
            {
                // Player is within follow range but too far to hit
                Move(directionX);
            }
            else if (distanceY <= heightTolerance)
            {
                // Player is close enough and at the right height
                Stop();
                Attack();
            }
            else
            {
                // Player is close but too high/low to reach
                Stop();
            }
        }
        else
        {
            // Player is out of range
            Stop();
        }
    }

    void Move(float directionX)
    {
        float move = Mathf.Sign(directionX);

        // Set physics velocity
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Set animator parameter
        if (anim != null)
        {
            anim.SetFloat("xVelocity", Mathf.Abs(move));
        }

        // Flip sprite based on movement direction
        if (move > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (move < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetFloat("xVelocity", 0f);
        }
    }

    void Attack()
    {
        if (isAttacking) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
            lastAttackTime = Time.time;
        }
    }

    
    public void EndAttack()
    {
        isAttacking = false;

        if (player != null)
        {
            
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform.position);
                }
            }
        }
    }
}