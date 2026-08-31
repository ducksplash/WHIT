using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScorpionController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 720f;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    
    [Header("Health")]
    public int maxHealth = 60;
    public int health = 60;
    public Slider healthBar;
    public Slider maxHealthBar;
    public Image healthBarFill;
    private bool isDead = false;

    [Header("References")]
    public Animator animator;
    public Transform player;
    [HideInInspector] public PortalController portal;

    private bool isAttacking = false;
    private float distanceToPlayer;

    [Header("FX")]
    public GameObject particlePoof;
    public float particleDuration = 3f;

    public Rigidbody rb;
    private bool isBeingKnockedBack = false;
    
    public float knockbackDuration = 0.3f;
    public Vector2 attackKnockbackForce = new Vector2(5f, 1f);
    public Vector2 meleeKnockbackForce = new Vector2(5f, 1f);

    void Start()
    {
        player = Player.Instance.transform;
        UpdateHealthBars();
    }

    void Update()
    {
        if (isDead || health <= 0) return;
        if (player == null || isAttacking) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", false);

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else if (distanceToPlayer <= chaseRange)
        {
            MoveTowardsPlayer(direction);
            animator.SetBool("IsWalking", true);
        }
        else
        {
            Idle();
            animator.SetBool("IsIdle", true);
        }

        RotateTowards(direction);
    }

    void MoveTowardsPlayer(Vector3 direction)
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.magnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
        }
    }

    void Idle()
    {
        animator.SetBool("IsIdle", true);
    }

    void Attack()
    {
        isAttacking = true;
        animator.SetBool("IsAttacking", true);
        isAttacking = false;
    }
    
    private void UpdateHealthBars()
    {
        if (healthBar != null)
        {
            float healthPercentage = (float)health / maxHealth;
            healthBar.value = healthPercentage;
            if (healthBarFill != null)
            {
                Color targetColor = healthPercentage > 0.5f ? Color.green : healthPercentage > 0.25f ? new Color(1f, 0.5f, 0f) : Color.red;
                targetColor.a = 1f;
                healthBarFill.color = targetColor;
            }
        }
        if (maxHealthBar != null)
        {
            maxHealthBar.value = 1f;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (health <= 0 || isDead) return;

        // if (other.CompareTag("PlayerMelee"))
        // {
        //     // int meleeDamage = other.GetComponent<Weapon>()?.baseDamage ?? 1;
        //     // animator.SetBool("WasHit", true);
        //     // StartCoroutine(ApplyKnockback());
        //     // health = Mathf.Max(0, health - meleeDamage);
        //     // UpdateHealthBars();
        //     // if (health <= 0)
        //     // {
        //     //     Die();
        //     // }
        // }
    }
    
    private IEnumerator ApplyKnockback()
    {
        if (isDead) yield break;

        if (isBeingKnockedBack) yield break;

        isBeingKnockedBack = true;
        rb.isKinematic = false;

        Vector3 direction = (transform.position - Player.Instance.transform.position).normalized;
        Vector3 knockback = direction * meleeKnockbackForce.x + Vector3.up * meleeKnockbackForce.y;
        rb.AddForce(knockback, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        animator.SetBool("WasHit", false);

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isBeingKnockedBack = false;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Scorpion died");

        StopAllCoroutines();

        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        if (animator != null)
        {
            animator.enabled = false;
            animator.applyRootMotion = false;
        }
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            Collider playerCollider = Player.Instance?.GetComponent<Collider>();
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(capsule, playerCollider, true);
            }
            capsule.enabled = false;
        }

        if (particlePoof != null)
        {
            GameObject poof = Instantiate(particlePoof, transform.position, Quaternion.identity);
            ParticleSystem ps = poof.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.duration = particleDuration;
                main.startLifetime = particleDuration;
                ps.Play();
            }
        }
        
        if (portal != null) portal.RemoveEnemy(gameObject);
        Destroy(gameObject);
    }
}