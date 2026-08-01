using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 10f;

    [Header("Visual Effects (Prefabs)")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private GameObject dashEffectPrefab;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [Range(0.1f, 3f)][SerializeField] private float minPitch = 0.9f;
    [Range(0.1f, 3f)][SerializeField] private float maxPitch = 1.1f;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerControls controls;
    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool wasGrounded;
    private bool canDoubleJump;

    private bool isHurt;
    public bool canMove = true;
    private bool canDash = true;
    private bool isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controls = new PlayerControls();
        audioSource = audioSource ?? GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Move.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>().x;
        controls.Player.Move.canceled += ctx => horizontalInput = 0;
        controls.Player.Jump.performed += ctx => OnJump();
        controls.Player.Dash.performed += ctx => OnDash();
    }

    private void OnDisable() => controls.Disable();

    private void Update()
    {
        CheckGround();
        UpdateAnimationParameters();

        if (isDashing || isHurt || !canMove) return;

        Flip();
    }

    private void FixedUpdate()
    {
        if (isDashing || isHurt || !canMove)
        {
            if (!isDashing && !isHurt) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    private void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded) canDoubleJump = true;

        // landing effect
        if (!wasGrounded && isGrounded)
        {
            SpawnImpactEffect(1.5f);
        }
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    private void OnJump()
    {
        if (!canMove || isDashing) return;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            canDoubleJump = true;
            SpawnImpactEffect(0.8f);
            PlayRandomizedSound(jumpSound); //jump sound
        }
        else if (canDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            canDoubleJump = false;
            if (anim != null) anim.SetTrigger("DoubleJump");
            SpawnImpactEffect(0.5f);
            PlayRandomizedSound(jumpSound); //double jump sound
        }
    }

    private void OnDash()
    {
        if (canDash && canMove && !isHurt)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        if (anim != null) anim.SetBool("isDashing", true);

        SpawnDashEffect();
        PlayRandomizedSound(dashSound); //dash sound

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        Vector2 dashDir = new Vector2(isFacingRight ? 1 : -1, 0);
        rb.linearVelocity = dashDir * dashForce;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        if (anim != null) anim.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void PlayRandomizedSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip);
        }
    }

    // dash, impact effect
    void SpawnImpactEffect(float scale = 1f)
    {
        if (impactEffectPrefab == null || groundCheck == null) return;
        GameObject effect = Instantiate(impactEffectPrefab, groundCheck.position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;
        Destroy(effect, 0.5f);
    }

    void SpawnDashEffect()
    {
        if (dashEffectPrefab == null) return;
        // spawn effect behind player base on player facing direction
        Vector3 dashDir = isFacingRight ? Vector3.right : Vector3.left;
        Vector3 spawnPos = transform.position - (dashDir * 0.5f);

        GameObject effect = Instantiate(dashEffectPrefab, spawnPos, Quaternion.identity);
        effect.transform.right = dashDir;
        Destroy(effect, 0.3f);
    }

    private void UpdateAnimationParameters()
    {
        if (anim == null) return;

        if (isHurt)
        {
            anim.SetFloat("xVelocity", 0f);
            anim.SetBool("isGrounded", true);
            return;
        }

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isDashing", isDashing);
    }

    private void Flip()
    {
        if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void LockPlayer() => canMove = false;
    public void UnlockPlayer() => canMove = true;
    public void SetHurt(bool state) => isHurt = state;
}